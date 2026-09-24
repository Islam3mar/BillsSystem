using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class CategoryInputValidator : AbstractValidator<CategoryInput>
    {
        public CategoryInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.ItemTypeId)
                .GreaterThan(0).WithMessage("TYPE NAME is Required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("CATEGORY NAME is Required")
                .MaximumLength(150).WithMessage("CATEGORY NAME must not exceed 150 characters");
        }
    }
}
