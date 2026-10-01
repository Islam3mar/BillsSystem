using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int BillId { get; set; }
        public Bill Bill { get; set; } = null!;

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }

        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidReason { get; set; }

        // ---------- جديد: بيانات الدفع أونلاين ----------
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
        public string? StripeSessionId { get; set; }          // بيستخدم للتأكد من عدم تكرار نفس الدفعة (Idempotency)
        public string? StripePaymentIntentId { get; set; }    // للرجوع له في لوحة تحكم Stripe عند الحاجة
    }
}
