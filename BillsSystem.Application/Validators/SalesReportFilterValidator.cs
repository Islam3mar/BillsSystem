using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using FluentValidation;

namespace BillsSystem.Application.Validators
{
    public class SalesReportFilterValidator : AbstractValidator<SalesReportFilter>
    {
        public SalesReportFilterValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.FromDate)
                .NotEqual(default(DateTime)).WithMessage("Start Period is Required");

            RuleFor(x => x.ToDate)
                .NotEqual(default(DateTime)).WithMessage("End Period is Required")
                .GreaterThanOrEqualTo(x => x.FromDate).WithMessage("End Period must be after or equal Start Period");
        }
    }
}
