using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Helpers;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class ClientInputValidator : AbstractValidator<ClientInput>
    {
        public ClientInputValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("CLIENT NAME is Required")
                .MaximumLength(150).WithMessage("CLIENT NAME must not exceed 150 characters");

            // ---------- Phone Number Validation ----------
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("PHONE is Required")
                .Length(11).WithMessage("PHONE must be exactly 11 digits")
                .Matches(@"^\d{11}$").WithMessage("PHONE must contain digits only")
                .Must(EgyptianMobilePrefixHelper.HasValidPrefix)
                    .WithMessage($"PHONE must start with one of the following prefixes: {string.Join(", ", EgyptianMobilePrefixHelper.AllPrefixStrings)}");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is Required")
                .MaximumLength(300).WithMessage("Address must not exceed 300 characters");
        }
    }
}
