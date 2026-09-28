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
        public decimal SellingPrice { get; set; }

        public DiscountType DiscountType { get; set; } = DiscountType.Value;
        public decimal Discount { get; set; }            // زي ما اتكتب: مبلغ أو نسبة

        // كلهم بيتحسبوا مرة واحدة وقت الحفظ (BillService) وبيتخزنوا
        public decimal Total { get; set; }               // SellingPrice * Quantity
        public decimal DiscountAmount { get; set; }      // قيمة الخصم بالمبلغ (حتى لو الخصم نسبة)
        public decimal Balance { get; set; }             // Total - DiscountAmount

        // ---------- Snapshot: نسخة من بيانات الصنف وقت البيع ----------
        public string ItemName { get; set; } = null!;
        public string TypeName { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string UnitName { get; set; } = null!;
        public decimal BuyingPrice { get; set; }
    }
}
