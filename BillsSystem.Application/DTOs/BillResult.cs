using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.DTOs
{
    public class BillItemRowError
    {
        public int Index { get; set; }
        public string? ItemError { get; set; }
        public string? QuantityError { get; set; }
        public string? SellingPriceError { get; set; }
        public string? DiscountError { get; set; }
    }

    public class BillResult
    {
        public bool Success { get; set; }
        public Bill? Bill { get; set; }

        public string? BillDateError { get; set; }
        public string? ClientError { get; set; }
        public string? ItemsError { get; set; }
        public string? PercentageDiscountError { get; set; }
        public string? PaidUpError { get; set; }
        public string? ValueDiscountError { get; set; }


        public List<BillItemRowError> ItemRowErrors { get; set; } = new();

        // وضيفه في HasErrors كمان:
        public bool HasErrors =>
            BillDateError != null || ClientError != null || ItemsError != null ||
            PercentageDiscountError != null || ValueDiscountError != null || PaidUpError != null ||
            ItemRowErrors.Any();
    }
}
