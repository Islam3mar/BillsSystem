using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class UnitInputValidator : AbstractValidator<UnitInput>
    {
        public UnitInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("UNIT NAME is Required")
                .MaximumLength(150).WithMessage("UNIT NAME must not exceed 150 characters");
        }
    }
}
