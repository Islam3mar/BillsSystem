using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Application.DTOs
{
    public class BillInput
    {
        public Guid? SubmissionId { get; set; }          // بيمنع تكرار نفس الفورم (Double-submit)

        public DateTime BillDate { get; set; }
        public int ClientId { get; set; }

        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;
        public decimal PercentageDiscount { get; set; }
        public decimal ValueDiscount { get; set; }

        public decimal PaidUp { get; set; }

        // ميعاد السداد (اختياري). بيتتجاهل لو الفاتورة Fully Paid
        public DateTime? DueDate { get; set; }
        public List<BillItemInput> Items { get; set; } = new();
    }
}
