using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Reports;

namespace BillsSystem.Domain.Interfaces
{
    public interface IBillRepository : IGenericRepository<Bill>
    {
        Task<int?> GetIdBySubmissionAsync(Guid submissionId);

        // نسخة للعرض فقط (AsNoTracking) - للـ Details، مش للتعديل
        Task<Bill?> GetByIdReadOnlyAsync(int id);

        Task<decimal> GetTotalOutstandingAsync();

        // مجموع المتبقي (TheRest) على عميل معين من كل فواتيره غير المحذوفة
        Task<decimal> GetClientOutstandingAsync(int clientId);

        // التقارير كلها بتتحسب في SQL (مفيش تحميل للفواتير في الذاكرة)
        Task<SalesTotalsRow> GetSalesTotalsAsync(DateTime from, DateTime to);
        Task<List<BillSummaryRow>> GetBillSummariesAsync(DateTime from, DateTime to);
        Task<List<ItemSalesRow>> GetTopItemsAsync(DateTime from, DateTime to, int take);
        Task<List<ClientSalesRow>> GetTopClientsAsync(DateTime from, DateTime to, int take);


        Task<bool> StripeSessionExistsAsync(string stripeSessionId);


        // الفاتورة (Tracked) اللي عليها دفعة بالـ PaymentIntent ده
        Task<Bill?> GetByStripePaymentIntentAsync(string paymentIntentId);


        // ---------- التذكيرات ----------
        // فواتير ليها DueDate وعليها متبقي وميعادها <= النهارده + daysBefore (الفلترة النهائية في ReminderService)
        Task<List<ReminderCandidateRow>> GetReminderCandidatesAsync(DateTime today, int daysBefore);
        Task<ReminderCandidateRow?> GetReminderCandidateAsync(int billId);

        // UPDATE مباشر لعمودين بس، من غير RowVersion/Tracking عشان مايتعارضش مع أدمن بيسجل دفعة
        Task MarkReminderSentAsync(int billId, ReminderType type, DateTime now);


        // محاولة فاشلة: بنسجل وقتها بس (مش بنعلّم إن التذكير اتبعت)
        Task MarkReminderAttemptAsync(int billId, DateTime now);
    }
}
