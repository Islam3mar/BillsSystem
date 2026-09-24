using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class ItemTypeInputValidator : AbstractValidator<ItemTypeInput>
    {
        public ItemTypeInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.CompanyId)
                .GreaterThan(0).WithMessage("COMPANY NAME is Required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("TYPE NAME is Required")
                .MaximumLength(150).WithMessage("TYPE NAME must not exceed 150 characters");
        }
    }
}
