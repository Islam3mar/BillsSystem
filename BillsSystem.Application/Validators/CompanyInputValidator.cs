using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class CompanyInputValidator : AbstractValidator<CompanyInput>
    {
        public CompanyInputValidator()
        {
            // بتوقف باقي الشروط لنفس الحقل أول ما شرط يفشل
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("COMPANY NAME is Required")
                .MaximumLength(150).WithMessage("COMPANY NAME must not exceed 150 characters");
        }
    }
}
