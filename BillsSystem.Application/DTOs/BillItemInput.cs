using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Application.DTOs
{
    public class BillItemInput
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public DiscountType DiscountType { get; set; } = DiscountType.Value;
        public decimal Discount { get; set; }
    }
}
