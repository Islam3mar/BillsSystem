using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Enums;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class BillItemInputValidator : AbstractValidator<BillItemInput>
    {
        public BillItemInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.ItemId)
                .GreaterThan(0).WithMessage("ITEM NAME is Required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity Must be Greater than Zero");

            RuleFor(x => x.SellingPrice)
                .GreaterThanOrEqualTo(0).WithMessage("SELLING PRICE Must be Greater than or equal Zero");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount Must be Greater than or equal Zero");

            RuleFor(x => x.Discount)
                .LessThanOrEqualTo(100).WithMessage("Discount percentage can't exceed 100")
                .When(x => x.DiscountType == DiscountType.Percentage);
        }
    }
}
