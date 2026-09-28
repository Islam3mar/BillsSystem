using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Reports;

namespace BillsSystem.Domain.Interfaces
{
    public interface IBillRepository : IGenericRepository<Bill>
    {
        Task<int?> GetIdBySubmissionAsync(Guid submissionId);
        Task<decimal> GetTotalOutstandingAsync();

        // التقارير كلها بتتحسب في SQL (مفيش تحميل للفواتير في الذاكرة)
        Task<SalesTotalsRow> GetSalesTotalsAsync(DateTime from, DateTime to);
        Task<List<BillSummaryRow>> GetBillSummariesAsync(DateTime from, DateTime to);
        Task<List<ItemSalesRow>> GetTopItemsAsync(DateTime from, DateTime to, int take);
        Task<List<ClientSalesRow>> GetTopClientsAsync(DateTime from, DateTime to, int take);
    }
}
