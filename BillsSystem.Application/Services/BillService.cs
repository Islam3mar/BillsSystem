using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Interfaces;
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

        public async Task<IEnumerable<Bill>> GetAllAsync()
            => await _unitOfWork.Bills.GetAllAsync();

        public async Task<Bill?> GetByIdAsync(int id)
            => await _unitOfWork.Bills.GetByIdAsync(id);

        public async Task<BillResult> CreateAsync(BillInput input)
        {
            var result = new BillResult();

            var validation = await _validator.ValidateAsync(input);
            if (!validation.IsValid)
            {
                MapValidationErrors(validation, result);
                return result;
            }

            // تأكيد إن الأصناف والعميل فعلاً موجودين (حماية من التلاعب في الـ Request)
            foreach (var row in input.Items)
            {
                var itemExists = await _unitOfWork.Items.GetByIdAsync(row.ItemId);
                if (itemExists == null)
                {
                    result.ItemsError = "One or more selected items no longer exist";
                    return result;
                }
            }

            var clientExists = await _unitOfWork.Clients.GetByIdAsync(input.ClientId);
            if (clientExists == null)
            {
                result.ClientError = "Selected client no longer exists";
                return result;
            }

            // ---------- الحسابات كلها هنا وبس  ----------
            var billItems = input.Items.Select(row => new BillItem
            {
                ItemId = row.ItemId,
                Quantity = row.Quantity,
                SellingPrice = row.SellingPrice,
                DiscountType = row.DiscountType,
                Discount = row.Discount
            }).ToList();

            var billsTotal = billItems.Sum(i => i.Balance);

            decimal percentageDiscount;
            decimal valueDiscount;

            if (input.DiscountType == DiscountType.Value)
            {
                // اللي دخله اليوزر هو اللي بيتخصم بالظبط - من غير أي تحويل وسيط
                valueDiscount = input.ValueDiscount;

                if (valueDiscount > billsTotal)
                {
                    result.ValueDiscountError = "Value discount can't exceed Bills Total";
                    return result;
                }

                // Percentage هنا بس رقم للعرض، مش بيتستخدم في أي حساب
                percentageDiscount = billsTotal > 0 ? Math.Round(valueDiscount / billsTotal * 100, 4) : 0;
            }
            else
            {
                percentageDiscount = input.PercentageDiscount;
                valueDiscount = billsTotal * percentageDiscount / 100m;
            }

            var theNet = Math.Max(0, billsTotal - valueDiscount);

            if (input.PaidUp > theNet)
            {
                result.PaidUpError = "Paid Up can't exceed The Net";
                return result;
            }

            var theRest = theNet - input.PaidUp;

            var bill = new Bill
            {
                BillDate = input.BillDate,
                ClientId = input.ClientId,
                Items = billItems,
                BillsTotal = billsTotal,
                DiscountType = input.DiscountType,
                PercentageDiscount = percentageDiscount,
                ValueDiscount = valueDiscount,
                TheNet = theNet,
                PaidUp = input.PaidUp,
                TheRest = theRest
            };

            await _unitOfWork.Bills.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Bill = bill;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var bill = await _unitOfWork.Bills.GetByIdAsync(id);
            if (bill == null) return (false, "Bill not found");

            _unitOfWork.Bills.Delete(bill);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "This bill can't be deleted");
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
