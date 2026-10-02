using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Domain.Specifications;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillsSystem.Application.Services
{
    public class BillService : IBillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<BillInput> _validator;
        private readonly ILogger<BillService> _logger;

        private readonly IStripeCheckoutService _stripeCheckoutService;
        private readonly INotificationService _notifications;
        private readonly IClientEmailService _clientEmails;
        private readonly ReminderSettings _reminderSettings;

        public BillService(IUnitOfWork unitOfWork, IValidator<BillInput> validator,
            ILogger<BillService> logger, IStripeCheckoutService stripeCheckoutService,
            INotificationService notifications, IClientEmailService clientEmails,
            IOptions<ReminderSettings> reminderSettings)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
            _logger = logger;
            _stripeCheckoutService = stripeCheckoutService;
            _notifications = notifications;
            _clientEmails = clientEmails;
            _reminderSettings = reminderSettings.Value;
        }

        public async Task<PagedResult<Bill>> GetPagedAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(page, 1);
            var spec = new BillsPagedSpecification(search, page, pageSize);

            var total = await _unitOfWork.Bills.CountAsync(spec);
            var items = (await _unitOfWork.Bills.ListAsync(spec)).ToList();

            return new PagedResult<Bill> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task<Bill?> GetByIdAsync(int id)
       => await _unitOfWork.Bills.GetByIdReadOnlyAsync(id);

        public async Task<BillResult> CreateAsync(BillInput input)
        {
            var result = new BillResult();

            // نفس الفورم اتبعت قبل كده → رجّع الفاتورة الأصلية وعلّم إنها "AlreadySaved"
            if (input.SubmissionId is Guid submissionId)
            {
                var existingId = await _unitOfWork.Bills.GetIdBySubmissionAsync(submissionId);
                if (existingId != null)
                {
                    result.Success = true;
                    result.AlreadySaved = true;
                    result.BillId = existingId;
                    return result;
                }
            }

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                MapValidationErrors(validation, result);
                return result;
            }

            var client = await _unitOfWork.Clients.GetByIdAsync(input.ClientId);
            if (client == null)
            {
                result.ClientError = "Selected client no longer exists";
                return result;
            }

            var itemIds = input.Items.Select(i => i.ItemId).Distinct().ToList();
            var items = (await _unitOfWork.Items.GetByIdsAsync(itemIds)).ToDictionary(i => i.Id);
            if (items.Count != itemIds.Count)
            {
                result.ItemsError = "One or more selected items no longer exist";
                return result;
            }

            // ---------- المخزون (نفس الصنف ممكن يتكرر في أكتر من صف فبنجمع الكميات) ----------
            var requested = input.Items.GroupBy(i => i.ItemId)
                                       .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity!.Value));
            foreach (var (itemId, qty) in requested)
            {
                var item = items[itemId];
                if (qty > item.QuantityInStock)
                {
                    result.ItemsError = $"Not enough stock for '{item.Name}'. Available: {item.QuantityInStock}";
                    return result;
                }
            }

            // ---------- الحسابات كلها هنا وبس، وكل مبلغ بيتقرّب مرة واحدة ----------
            var lines = new List<BillItem>();
            foreach (var row in input.Items)
            {
                var item = items[row.ItemId];
                var qty = row.Quantity!.Value;
                var price = Money.Round(row.SellingPrice);
                var discount = Math.Round(row.Discount, 2, MidpointRounding.AwayFromZero);

                var total = Money.Round(price * qty);
                var discountAmount = Math.Min(total, Money.Round(row.DiscountType == DiscountType.Percentage
                    ? total * discount / 100m : discount));

                lines.Add(new BillItem
                {
                    ItemId = item.Id,
                    Quantity = qty,
                    SellingPrice = price,
                    DiscountType = row.DiscountType,
                    Discount = discount,
                    Total = total,
                    DiscountAmount = discountAmount,
                    Balance = total - discountAmount,

                    // Snapshot
                    ItemName = item.Name,
                    TypeName = item.ItemType.Name,
                    CompanyName = item.ItemType.Company.Name,
                    UnitName = item.Unit.Name,
                    BuyingPrice = item.BuyingPrice
                });
            }

            var billsTotal = lines.Sum(l => l.Balance);

            decimal percentageDiscount;
            decimal valueDiscount;

            if (input.DiscountType == DiscountType.Value)
            {
                valueDiscount = Money.Round(input.ValueDiscount);
                if (valueDiscount > billsTotal)
                {
                    result.ValueDiscountError = "Value discount can't exceed Bills Total";
                    return result;
                }
                percentageDiscount = billsTotal > 0 ? Money.RoundPercent(valueDiscount / billsTotal * 100m) : 0;
            }
            else
            {
                percentageDiscount = Money.RoundPercent(input.PercentageDiscount);
                valueDiscount = Money.Round(billsTotal * percentageDiscount / 100m);
            }

            var theNet = billsTotal - valueDiscount;

            var paidUp = Money.Round(input.PaidUp);
            if (paidUp > theNet)
            {
                result.PaidUpError = "Paid Up can't exceed The Net";
                return result;
            }

            // ---------- سقف الدين: الفاتورة اللي هتزوّد الدين ومش هتتحفظ لو هتعدّي الحد ----------
            var newDebt = theNet - paidUp;
            decimal currentDebt = 0;
            if (client.MaxCreditLimit is decimal limit && newDebt > 0)
            {
                currentDebt = await _unitOfWork.Bills.GetClientOutstandingAsync(client.Id);
                if (currentDebt + newDebt > limit)
                {
                    var available = Math.Max(0, limit - currentDebt);
                    result.ClientError = $"Credit limit exceeded for '{client.Name}'. Limit: {limit:0.00}, current debt: {currentDebt:0.00}, " +
                                         $"this bill adds: {newDebt:0.00}. Available credit: {available:0.00}. Increase Paid Up or ask the client to settle part of the debt first";
                    await _notifications.NotifyAsync(NotificationType.Client, NotificationAction.Alert, $"Bill rejected: '{client.Name}' exceeded the credit limit ({limit:0.00}). Current debt: {currentDebt:0.00}, this bill adds: {newDebt:0.00}", client.Id);
                    await TryEmailClientCreditAsync(client, limit, currentDebt);
                    return result;
                }
            }

            var billDate = input.BillDate.Date;
            var bill = new Bill
            {
                SubmissionId = input.SubmissionId,
                BillDate = billDate,
                ClientId = input.ClientId,
                Items = lines,
                BillsTotal = billsTotal,
                DiscountType = input.DiscountType,
                PercentageDiscount = percentageDiscount,
                ValueDiscount = valueDiscount,
                TheNet = theNet,
                PaidUp = paidUp,
                TheRest = theNet - paidUp,

                // ميعاد السداد بيتحفظ بس لو فيه متبقي فعلًا
                DueDate = newDebt > 0 ? input.DueDate?.Date : null
            };

            if (paidUp > 0)
                bill.Payments.Add(new Payment { Amount = paidUp, PaymentDate = billDate, Notes = "Initial payment" });

            foreach (var (itemId, qty) in requested)
                items[itemId].QuantityInStock -= qty;

            await _unitOfWork.Bills.AddAsync(bill);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Stock concurrency conflict while saving bill for client {ClientId}", input.ClientId);
                result.ItemsError = "Stock changed while saving. Please review the items and try again";
                return result;
            }
            catch (DbUpdateException ex)
            {
                if (input.SubmissionId is Guid sid)
                {
                    var existingId = await _unitOfWork.Bills.GetIdBySubmissionAsync(sid);
                    if (existingId != null)
                    {
                        _logger.LogInformation("Duplicate submission {SubmissionId} resolved to bill {BillId}", sid, existingId);
                        result.Success = true;
                        result.AlreadySaved = true;
                        result.BillId = existingId;
                        return result;
                    }
                }

                _logger.LogError(ex, "Failed to save bill for client {ClientId} (SubmissionId {SubmissionId})",
                    input.ClientId, input.SubmissionId);
                result.GeneralError = "Couldn't save the invoice. Please try again";
                return result;
            }

            result.Success = true;
            result.Bill = bill;
            await _notifications.NotifyAsync(NotificationType.Bill, NotificationAction.Created, $"Bill #{bill.Id} created for '{client.Name}' - Net: {theNet:0.00}, Rest: {bill.TheRest:0.00}", bill.Id);
            result.BillId = bill.Id;

            // الدين بعد الفاتورة وصل (أو قرّب من) سقف العميل → إيميل تنبيه له
            if (client.MaxCreditLimit is decimal creditLimit && newDebt > 0)
            {
                var debtAfter = currentDebt + newDebt;
                if (debtAfter >= creditLimit * _reminderSettings.CreditWarningPercent / 100m)
                    await TryEmailClientCreditAsync(client, creditLimit, debtAfter);
            }

            return result;
        }

        // Soft delete: الفاتورة بتفضل في الداتابيز، والمخزون بيرجع
        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var bill = await _unitOfWork.Bills.GetByIdAsync(id);
            if (bill == null) return (false, "Bill not found");

            var items = await _unitOfWork.Items.GetByIdsAsync(bill.Items.Select(l => l.ItemId));
            foreach (var line in bill.Items)
            {
                var item = items.FirstOrDefault(i => i.Id == line.ItemId);
                if (item != null) item.QuantityInStock += line.Quantity;
            }

            bill.IsDeleted = true;
            bill.DeletedAt = AppClock.Now;

            try
            {
                await _unitOfWork.SaveChangesAsync();
                await _notifications.NotifyAsync(NotificationType.Bill, NotificationAction.Deleted, $"Bill #{bill.Id} for '{bill.Client.Name}' was deleted", bill.Id);
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while deleting bill {BillId}", id);
                return (false, "This bill was changed by someone else. Reload and try again");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete bill {BillId}", id);
                return (false, "This bill can't be deleted");
            }
        }

        public async Task<(bool Success, string? Error)> AddPaymentAsync(int billId, decimal amount, DateTime? paymentDate, string? notes)
        {
            var bill = await _unitOfWork.Bills.GetByIdAsync(billId);
            if (bill == null) return (false, "Bill not found");

            amount = Money.Round(amount);
            if (amount <= 0) return (false, "Payment amount Must be Greater than Zero");
            if (amount > bill.TheRest) return (false, $"Payment can't exceed The Rest ({bill.TheRest:0.00})");

            var date = (paymentDate ?? AppClock.Today).Date;
            if (date < bill.BillDate.Date || date > AppClock.Today)
                return (false, "Payment date must be between the bill date and today");

            notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            if (notes != null && notes.Length > 200) return (false, "Notes must not exceed 200 characters");

            bill.Payments.Add(new Payment { Amount = amount, PaymentDate = date, Notes = notes });
            bill.PaidUp += amount;
            bill.TheRest -= amount;

            try
            {
                await _unitOfWork.SaveChangesAsync();
                await _notifications.NotifyAsync(NotificationType.Payment, NotificationAction.Created, $"Payment of {amount:0.00} added to bill #{bill.Id} ({bill.Client.Name})", bill.Id);
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while adding payment to bill {BillId}", billId);
                return (false, "This bill was changed by someone else. Reload and try again");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add payment to bill {BillId}", billId);
                return (false, "Couldn't save the payment. Please try again");
            }
        }

        // إلغاء دفعة غلط: بتفضل ظاهرة في السجل (Voided) بس مش بتتحسب
        public async Task<(bool Success, string? Error)> VoidPaymentAsync(int billId, int paymentId, string? reason)
        {
            var bill = await _unitOfWork.Bills.GetByIdAsync(billId);
            if (bill == null) return (false, "Bill not found");

            var payment = bill.Payments.FirstOrDefault(p => p.Id == paymentId);
            if (payment == null) return (false, "Payment not found");
            if (payment.IsVoided) return (false, "This payment is already voided");

            reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
            if (reason == null) return (false, "Void reason is required");
            if (reason.Length > 200) return (false, "Reason must not exceed 200 characters");

            payment.IsVoided = true;
            payment.VoidedAt = AppClock.Now;
            payment.VoidReason = reason;

            bill.PaidUp -= payment.Amount;
            bill.TheRest += payment.Amount;

            try
            {
                await _unitOfWork.SaveChangesAsync();
                await _notifications.NotifyAsync(NotificationType.Payment, NotificationAction.Alert, $"Payment of {payment.Amount:0.00} voided on bill #{bill.Id} ({bill.Client.Name})", bill.Id);
                // إلغاء الدفعة زوّد دين العميل: لو وصل لنسبة التحذير أو عدّى السقف، نبلّغ الأدمن والعميل
                await NotifyIfCreditLimitReachedAsync(bill.Client);
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict while voiding payment {PaymentId}", paymentId);
                return (false, "This bill was changed by someone else. Reload and try again");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to void payment {PaymentId} on bill {BillId}", paymentId, billId);
                return (false, "Couldn't void the payment. Please try again");
            }
        }

        public async Task<(bool Success, string? Error, string? CheckoutUrl)> CreateStripeCheckoutAsync(
    int billId, decimal amount, string successUrl, string cancelUrl)
        {
            var bill = await _unitOfWork.Bills.GetByIdAsync(billId);
            if (bill == null) return (false, "Bill not found", null);

            amount = Money.Round(amount);
            if (amount <= 0) return (false, "Payment amount Must be Greater than Zero", null);
            if (amount > bill.TheRest) return (false, $"Payment can't exceed The Rest ({bill.TheRest:0.00})", null);

            var (sessionId, url) = await _stripeCheckoutService.CreateCheckoutSessionAsync(billId, amount, successUrl, cancelUrl);
            _logger.LogInformation("Stripe checkout session {SessionId} created for bill {BillId}, amount {Amount}",
                sessionId, billId, amount);

            return (true, null, url);
        }

        //------------------------------------------------------------------------------------------
        // بتتنادى من الـ Webhook بس — مش من أي Controller Action عادي
        public async Task<(bool Success, string? Error)> ConfirmStripePaymentAsync(
            string stripeSessionId, string? stripePaymentIntentId, decimal amount, int billId)
        {
            // Idempotency: لو Stripe بعتت نفس الـ Event مرتين، متسجلش الدفعة مرتين
            if (await _unitOfWork.Bills.StripeSessionExistsAsync(stripeSessionId))
            {
                _logger.LogInformation("Stripe session {SessionId} already processed, skipping", stripeSessionId);
                return (true, null);
            }

            var bill = await _unitOfWork.Bills.GetByIdAsync(billId);
            if (bill == null)
            {
                _logger.LogError("Stripe webhook: bill {BillId} not found for session {SessionId}", billId, stripeSessionId);
                return (false, "Bill not found");
            }

            amount = Money.Round(amount);

            // الفلوس اتقبضت فعليًا، فلازم تتسجل حتى لو في سباق نادر زادت عن TheRest الحالي
            if (amount > bill.TheRest)
                _logger.LogWarning("Stripe payment {Amount} exceeds TheRest {TheRest} for bill {BillId} — recorded in full, needs review",
                    amount, bill.TheRest, billId);

            bill.Payments.Add(new Payment
            {
                Amount = amount,
                PaymentDate = AppClock.Today,
                Notes = "Paid via Stripe",
                Method = PaymentMethod.Stripe,
                StripeSessionId = stripeSessionId,
                StripePaymentIntentId = stripePaymentIntentId
            });

            bill.PaidUp += amount;
            bill.TheRest = Math.Max(0, bill.TheRest - amount);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                await _notifications.NotifyAsync(NotificationType.Payment, NotificationAction.Created, $"Stripe payment of {amount:0.00} received for bill #{bill.Id} ({bill.Client.Name})", bill.Id);
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save Stripe payment for bill {BillId}, session {SessionId}", billId, stripeSessionId);
                return (false, "Database error");
            }
        }

        //------------------------------------------------------------------------------------------


        // إيميل للعميل: وصل (أو قرّب من) سقف الدين. فيه Cooldown عشان منبعتش إيميل مع كل فاتورة، وفشله مبيكسرش العملية
        private async Task TryEmailClientCreditAsync(Client client, decimal limit, decimal currentDebt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(client.Email) || !_clientEmails.IsEnabled) return;

                var now = AppClock.Now;
                if (client.LastCreditLimitEmailAt is DateTime last &&
                    (now - last).TotalHours < _reminderSettings.CreditEmailCooldownHours) return;

                if (!await _clientEmails.SendCreditLimitAsync(client.Email, client.Name, limit, currentDebt)) return;

                client.LastCreditLimitEmailAt = now;
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Couldn't send the credit-limit email to client {ClientId}", client.Id);
            }
        }

        // بعد أي عملية بتزوّد دين العميل (زي إلغاء دفعة): لو الدين وصل لنسبة التحذير أو عدّى السقف
        // → تنبيه داخلي للأدمن + إيميل للعميل (بنفس الـ Cooldown). مبتمنعش العملية، وفشلها مبيكسرهاش
        private async Task NotifyIfCreditLimitReachedAsync(Client client)
        {
            try
            {
                if (client.MaxCreditLimit is not decimal limit) return;

                var debt = await _unitOfWork.Bills.GetClientOutstandingAsync(client.Id);
                if (debt < limit * _reminderSettings.CreditWarningPercent / 100m) return;

                var state = debt > limit ? "exceeded" : debt >= limit ? "reached" : "is close to";
                await _notifications.NotifyAsync(NotificationType.Client, NotificationAction.Alert,
                    $"'{client.Name}' {state} the credit limit after a debt increase. Current debt: {debt:0.00}, limit: {limit:0.00}", client.Id);

                await TryEmailClientCreditAsync(client, limit, debt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Couldn't run the credit-limit check for client {ClientId}", client.Id);
            }
        }

        private static void MapValidationErrors(FluentValidation.Results.ValidationResult validation, BillResult result)
        {
            var rowErrors = new Dictionary<int, BillItemRowError>();

            foreach (var error in validation.Errors)
            {
                if (error.PropertyName.StartsWith("Items["))
                {
                    var start = error.PropertyName.IndexOf('[') + 1;
                    var end = error.PropertyName.IndexOf(']');
                    var index = int.Parse(error.PropertyName.Substring(start, end - start));
                    var field = error.PropertyName.Substring(error.PropertyName.IndexOf('.') + 1);

                    if (!rowErrors.TryGetValue(index, out var rowError))
                    {
                        rowError = new BillItemRowError { Index = index };
                        rowErrors[index] = rowError;
                    }

                    switch (field)
                    {
                        case nameof(BillItemInput.ItemId): rowError.ItemError = error.ErrorMessage; break;
                        case nameof(BillItemInput.Quantity): rowError.QuantityError = error.ErrorMessage; break;
                        case nameof(BillItemInput.SellingPrice): rowError.SellingPriceError = error.ErrorMessage; break;
                        case nameof(BillItemInput.Discount): rowError.DiscountError = error.ErrorMessage; break;
                    }
                    continue;
                }

                switch (error.PropertyName)
                {
                    case nameof(BillInput.BillDate): result.BillDateError = error.ErrorMessage; break;
                    case nameof(BillInput.ClientId): result.ClientError = error.ErrorMessage; break;
                    case nameof(BillInput.Items): result.ItemsError = error.ErrorMessage; break;
                    case nameof(BillInput.PercentageDiscount): result.PercentageDiscountError = error.ErrorMessage; break;
                    case nameof(BillInput.PaidUp): result.PaidUpError = error.ErrorMessage; break;
                    case nameof(BillInput.ValueDiscount): result.ValueDiscountError = error.ErrorMessage; break;
                    case nameof(BillInput.DueDate): result.DueDateError = error.ErrorMessage; break;
                }
            }

            result.ItemRowErrors = rowErrors.Values.OrderBy(r => r.Index).ToList();
        }
    }
}
