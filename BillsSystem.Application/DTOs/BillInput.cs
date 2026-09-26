using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Application.DTOs
{
    public class BillInput
    {
        public DateTime BillDate { get; set; }
        public int ClientId { get; set; }

        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
        public decimal PercentageDiscount { get; set; }   // يُستخدم فقط لو DiscountType == Percentage
        public decimal ValueDiscount { get; set; }         // يُستخدم فقط لو DiscountType == Value

        public decimal PaidUp { get; set; }
        public List<BillItemInput> Items { get; set; } = new();
    }
}
