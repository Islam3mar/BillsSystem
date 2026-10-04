using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface IBillService
    {
        Task<PagedResult<Bill>> GetPagedAsync(string? search, int page, int pageSize);
        Task<Bill?> GetByIdAsync(int id);
        Task<BillResult> CreateAsync(BillInput input);
        Task<(bool Success, string? Error)> DeleteAsync(int id);        // Soft delete + إرجاع المخزون
        Task<(bool Success, string? Error)> AddPaymentAsync(int billId, decimal amount, DateTime? paymentDate, string? notes);
        Task<(bool Success, string? Error)> VoidPaymentAsync(int billId, int paymentId, string? reason);
        Task<(bool Success, string? Error)> SetDueDateAsync(int billId, DateTime dueDate);

        Task<(bool Success, string? Error, string? CheckoutUrl)> CreateStripeCheckoutAsync(
    int billId, decimal amount, string successUrl, string cancelUrl);

        Task<(bool Success, string? Error)> ConfirmStripePaymentAsync(
            string stripeSessionId, string? stripePaymentIntentId, decimal amount, int billId);


        // هل الدفعة بتاعة الـ Session دي اتسجلت (بالـ Webhook) على الفاتورة دي؟
        Task<bool> IsStripePaymentRecordedAsync(int billId, string stripeSessionId);



        // استرداد من Stripe: لو كامل بنلغي الدفعة تلقائيًا، لو جزئي بنبلّغ الأدمن
        Task<(bool Success, string? Error)> HandleStripeRefundAsync(string paymentIntentId, decimal totalRefunded);

        // اعتراض على دفعة: تنبيه فقط
        Task<(bool Success, string? Error)> HandleStripeDisputeAsync(string? paymentIntentId, decimal amount, string? reason);
    }
}
