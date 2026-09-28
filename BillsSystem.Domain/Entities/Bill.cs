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

        public DiscountType DiscountType { get; set; }
        public decimal PercentageDiscount { get; set; }
        public decimal ValueDiscount { get; set; }

        public decimal TheNet { get; set; }
        public decimal PaidUp { get; set; }      // = مجموع Payments (بيتحدث مع كل دفعة)
        public decimal TheRest { get; set; }     // = TheNet - PaidUp

        // Soft delete: الفاتورة بتفضل في الداتابيز بس بتختفي من كل الشاشات والتقارير
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // بيمنع إن نفس الفورم يتسجل مرتين (Double-submit)
        public Guid? SubmissionId { get; set; }

        // بيمنع تعديلين في نفس اللحظة على نفس الفاتورة (مثلاً دفعتين متزامنتين)
        public byte[] RowVersion { get; set; } = null!;

        public ICollection<BillItem> Items { get; set; } = new List<BillItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
