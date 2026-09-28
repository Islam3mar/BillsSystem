using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class BillItemInputValidator : AbstractValidator<BillItemInput>
    {
        // بيقصّ السعر والكمية على الحدود قبل الضرب، عشان الضرب عمره ما يعمل Overflow
        // (أي رقم فوق الحد بيتترفض من قاعدته الأصلية تحت)
        private static decimal SafeTotal(decimal price, int? qty)
        {
            var p = Math.Min(Math.Max(price, 0m), Limits.MaxPrice);
            var q = Math.Min(Math.Max(qty ?? 0, 0), Limits.MaxQuantity);
            return Money.Round(Money.Round(p) * q);   // نفس معادلة BillService بالظبط
        }

        public BillItemInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("ITEM NAME is Required");

            RuleFor(x => x.Quantity)
                .NotNull().WithMessage("Quantity is Required")
                .GreaterThan(0).WithMessage("Quantity Must be Greater than Zero")
                .LessThanOrEqualTo(Limits.MaxQuantity).WithMessage($"Quantity can't exceed {Limits.MaxQuantity:N0}")
                .Must((row, qty) => SafeTotal(row.SellingPrice, qty) <= Limits.MaxLineTotal)
                    .WithMessage("Item total is too large");

            RuleFor(x => x.SellingPrice)
                .GreaterThanOrEqualTo(0).WithMessage("SELLING PRICE Must be Greater than or equal Zero")
                .LessThanOrEqualTo(Limits.MaxPrice).WithMessage($"SELLING PRICE can't exceed {Limits.MaxPrice:N0}");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount Must be Greater than or equal Zero");

            RuleFor(x => x.Discount)
                .LessThanOrEqualTo(100).WithMessage("Discount percentage can't exceed 100")
                .When(x => x.DiscountType == DiscountType.Percentage);

            // خصم القيمة ما ينفعش يزيد عن إجمالي الصنف (بعد التقريب زي السيرفر بالظبط)
            RuleFor(x => x.Discount)
                .Must((row, d) => Money.Round(d) <= SafeTotal(row.SellingPrice, row.Quantity))
                .WithMessage("Discount can't exceed the item total")
                .When(x => x.DiscountType == DiscountType.Value);
        }
    }
}
