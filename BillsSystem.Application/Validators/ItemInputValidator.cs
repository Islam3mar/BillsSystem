using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class ItemInputValidator : AbstractValidator<ItemInput>
    {
        public ItemInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.ItemTypeId)
                .GreaterThan(0).WithMessage("TYPE NAME is Required");

            RuleFor(x => x.UnitId)
                .GreaterThan(0).WithMessage("UNIT NAME is Required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("ITEM NAME is Required")
                .MaximumLength(150).WithMessage("ITEM NAME must not exceed 150 characters");

            RuleFor(x => x.SellingPrice)
                .GreaterThanOrEqualTo(0).WithMessage("SELLING PRICE Must be Greater than or equal Zero");

            RuleFor(x => x.BuyingPrice)
                .GreaterThanOrEqualTo(0).WithMessage("BUYING PRICE Must be Greater than or equal Zero")
                .LessThanOrEqualTo(x => x.SellingPrice).WithMessage("BUYING PRICE Must be less than or equal SELLING PRICE");
        }
    }
}
