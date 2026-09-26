using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
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
                .NotEqual(default(DateTime)).WithMessage("BILL DATE is Required");

            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("CLIENT NAME is Required");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("You must add at least one item to the bill");

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
        }
    }
}
