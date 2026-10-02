using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class BillInputValidator : AbstractValidator<BillInput>
    {
        public BillInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.BillDate)
                .NotEqual(default(DateTime)).WithMessage("BILL DATE is Required")
                .Must(d => d.Year >= 2000 && d.Date <= AppClock.Today.AddDays(1))
                    .WithMessage("BILL DATE is out of the allowed range");

            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("CLIENT NAME is Required");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("You must add at least one item to the bill")
                .Must(i => i.Count <= 200).WithMessage("A bill can't have more than 200 lines");

            RuleForEach(x => x.Items).SetValidator(new BillItemInputValidator());

            RuleFor(x => x.PercentageDiscount)
                .GreaterThanOrEqualTo(0).WithMessage("Percentage discount Must be Greater than or equal Zero")
                .LessThanOrEqualTo(100).WithMessage("Percentage discount can't exceed 100")
                .When(x => x.DiscountType == DiscountType.Percentage);

            RuleFor(x => x.ValueDiscount)
                .GreaterThanOrEqualTo(0).WithMessage("Value discount Must be Greater than or equal Zero")
                .When(x => x.DiscountType == DiscountType.Value);

            RuleFor(x => x.PaidUp)
                .GreaterThanOrEqualTo(0).WithMessage("Paid Up Must be Greater than or equal Zero");

            // اختياري: ميعاد السداد لازم يكون من تاريخ الفاتورة وطالع
            RuleFor(x => x.DueDate)
                .Must((x, due) => due!.Value.Date >= x.BillDate.Date)
                    .WithMessage("DUE DATE can't be before the bill date")
                .Must((x, due) => due!.Value.Date <= x.BillDate.Date.AddYears(5))
                    .WithMessage("DUE DATE is too far from the bill date")
                .When(x => x.DueDate.HasValue);
        }
    }
}
