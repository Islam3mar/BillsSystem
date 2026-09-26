using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Domain.Entities
{
    public class Bill : BaseEntity
    {
        public DateTime BillDate { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public decimal BillsTotal { get; set; }

        public DiscountType DiscountType { get; set; }       // <-- جديد: أنهي حقل كان هو المصدر
        public decimal PercentageDiscount { get; set; }      // مخزّنة دايمًا للعرض (سواء مصدر أو محسوبة)
        public decimal ValueDiscount { get; set; }            // مخزّنة دايمًا للعرض (سواء مصدر أو محسوبة)

        public decimal TheNet { get; set; }
        public decimal PaidUp { get; set; }
        public decimal TheRest { get; set; }

        public ICollection<BillItem> Items { get; set; } = new List<BillItem>();
    }
}
