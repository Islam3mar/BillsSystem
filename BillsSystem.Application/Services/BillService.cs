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

namespace BillsSystem.Application.Services
{
    public class BillService : IBillService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<BillInput> _validator;

        public BillService(IUnitOfWork unitOfWork, IValidator<BillInput> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
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
            => await _unitOfWork.Bills.GetByIdAsync(id);

        public async Task<BillResult> CreateAsync(BillInput input)
        {
            var result = new BillResult();

            // نفس الفورم اتبعت قبل كده (Double-click / Refresh) → رجّع الفاتورة الأصلية بدل ما تعمل واحدة تانية
            if (input.SubmissionId is Guid submissionId)
            {
                var existingId = await _unitOfWork.Bills.GetIdBySubmissionAsync(submissionId);
                if (existingId != null)
                {
                    result.Success = true;
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

            // Query واحد لكل الأصناف (بدل Query لكل صنف)
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
                    Balance = total - discountAmount,     // مضمون ≥ 0 بفضل الـ Validator

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

            var theNet = billsTotal - valueDiscount;          // ≥ 0 دايمًا، فمفيش Math.Max بتخبي مشكلة

            var paidUp = Money.Round(input.PaidUp);
            if (paidUp > theNet)
            {
                result.PaidUpError = "Paid Up can't exceed The Net";
                return result;
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
                TheRest = theNet - paidUp
            };

            if (paidUp > 0)
                bill.Payments.Add(new Payment { Amount = paidUp, PaymentDate = billDate, Notes = "Initial payment" });

            // خصم المخزون في نفس الـ SaveChanges (Transaction واحدة مع الفاتورة)
            foreach (var (itemId, qty) in requested)
                items[itemId].QuantityInStock -= qty;

            await _unitOfWork.Bills.AddAsync(bill);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                result.ItemsError = "Stock changed while saving. Please review the items and try again";
                return result;
            }
            catch (DbUpdateException)
            {
                // ممكن يكون Double-submit سبق (Unique Index على SubmissionId)
                if (input.SubmissionId is Guid sid)
                {
                    var existingId = await _unitOfWork.Bills.GetIdBySubmissionAsync(sid);
                    if (existingId != null)
                    {
                        result.Success = true;
                        result.BillId = existingId;
                        return result;
                    }
                }
                result.GeneralError = "Couldn't save the invoice. Please try again";
                return result;
            }

            result.Success = true;
            result.Bill = bill;
            result.BillId = bill.Id;
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
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "This bill was changed by someone else. Reload and try again");
            }
            catch (DbUpdateException)
            {
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

            notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            if (notes != null && notes.Length > 200) return (false, "Notes must not exceed 200 characters");

            bill.Payments.Add(new Payment
            {
                Amount = amount,
                PaymentDate = (paymentDate ?? AppClock.Today).Date,
                Notes = notes
            });
            bill.PaidUp += amount;
            bill.TheRest -= amount;

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "This bill was changed by someone else. Reload and try again");
            }
            catch (DbUpdateException)
            {
                return (false, "Couldn't save the payment. Please try again");
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
                }
            }

            result.ItemRowErrors = rowErrors.Values.OrderBy(r => r.Index).ToList();
        }
    }
}
