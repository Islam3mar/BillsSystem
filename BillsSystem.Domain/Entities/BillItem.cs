using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Domain.Entities
{
    public class BillItem : BaseEntity
    {
        public int BillId { get; set; }
        public Bill Bill { get; set; } = null!;

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }          // نسخة من سعر البيع وقت البيع (قابل للتعديل قبل الإضافة)

        public DiscountType DiscountType { get; set; } = DiscountType.Value;
        public decimal Discount { get; set; }               // Value: مبلغ مباشر / Percentage: نسبة من 0 لـ 100

        // خصائص محسوبة، مش متخزنة (Ignored في الـ Configuration)
        public decimal Total => SellingPrice * Quantity;

        public decimal Balance => DiscountType == DiscountType.Percentage
            ? Total - (Total * Discount / 100m)
            : Total - Discount;
    }
}
