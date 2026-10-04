using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Domain.Reports;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public class BillRepository : BaseRepository<Bill>, IBillRepository
    {
        public BillRepository(ApplicationDbContext context) : base(context) { }

        // Tracked: بتُستخدم للعرض وللحذف وتسجيل الدفعات. الأصناف عندها Snapshot فمفيش Include للـ Item
        public override async Task<Bill?> GetByIdAsync(int id)
            => await Query.Include(b => b.Client)
                          .Include(b => b.Items)
                          .Include(b => b.Payments)
                          .AsSplitQuery()
                          .FirstOrDefaultAsync(b => b.Id == id);

        // AsNoTracking: بس للعرض (صفحة Details)، أخف على الذاكرة والأداء
        public async Task<Bill?> GetByIdReadOnlyAsync(int id)
            => await Query.Include(b => b.Client)
                          .Include(b => b.Items)
                          .Include(b => b.Payments)
                          .AsSplitQuery()
                          .AsNoTracking()
                          .FirstOrDefaultAsync(b => b.Id == id);


        public async Task<int?> GetIdBySubmissionAsync(Guid submissionId)
            => await Query.AsNoTracking()
                          .Where(b => b.SubmissionId == submissionId)
                          .Select(b => (int?)b.Id)
                          .FirstOrDefaultAsync();

        public async Task<decimal> GetTotalOutstandingAsync()
            => await Query.AsNoTracking().SumAsync(b => b.TheRest);

        public async Task<decimal> GetClientOutstandingAsync(int clientId)
            => await Query.AsNoTracking().Where(b => b.ClientId == clientId).SumAsync(b => b.TheRest);

        // ---------------- Reminders ----------------
        private static readonly Expression<Func<Bill, ReminderCandidateRow>> ToReminderRow = b => new ReminderCandidateRow
        {
            BillId = b.Id,
            BillDate = b.BillDate,
            DueDate = b.DueDate!.Value,
            TheNet = b.TheNet,
            TheRest = b.TheRest,
            LastReminderSentAt = b.LastReminderSentAt,
            LastReminderType = b.LastReminderType,
            LastReminderAttemptAt = b.LastReminderAttemptAt,
            ClientId = b.ClientId,
            ClientName = b.Client.Name,
            ClientEmail = b.Client.Email
        };
        public async Task<List<ReminderCandidateRow>> GetReminderCandidatesAsync(DateTime today, int daysBefore)
        {
            var horizon = today.Date.AddDays(Math.Max(0, daysBefore));

            // اللي عمره ما اتحاول (أو اتحاول من زمان) الأول، عشان الفاتورة الفاشلة متحجبش اللي وراها
            return await Query.AsNoTracking()
                .Where(b => b.DueDate != null && b.TheRest > 0 && b.DueDate <= horizon)
                .OrderBy(b => b.LastReminderAttemptAt ?? DateTime.MinValue)
                .ThenBy(b => b.DueDate).ThenBy(b => b.Id)
                .Select(ToReminderRow)
                .ToListAsync();
        }

        public async Task<ReminderCandidateRow?> GetReminderCandidateAsync(int billId)
            => await Query.AsNoTracking()
                .Where(b => b.Id == billId && b.DueDate != null)
                .Select(ToReminderRow)
                .FirstOrDefaultAsync();

        public async Task MarkReminderSentAsync(int billId, ReminderType type, DateTime now)
     => await Query.Where(b => b.Id == billId)
         .ExecuteUpdateAsync(s => s
             .SetProperty(b => b.LastReminderSentAt, now)
             .SetProperty(b => b.LastReminderType, type)
             .SetProperty(b => b.LastReminderAttemptAt, (DateTime?)null));

        public async Task MarkReminderAttemptAsync(int billId, DateTime now)
            => await Query.Where(b => b.Id == billId)
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.LastReminderAttemptAt, now));

        // ---------------- Reports (كلها SQL) ----------------
        private static (DateTime Start, DateTime EndExclusive) Range(DateTime from, DateTime to)
            => (from.Date, to.Date.AddDays(1));

        public async Task<SalesTotalsRow> GetSalesTotalsAsync(DateTime from, DateTime to)
        {
            var (start, end) = Range(from, to);

            // GroupBy(_ => 1) بيرجّع صف واحد بالكتير، فـ ToListAsync + FirstOrDefault بيشيل تحذير "من غير OrderBy"
            var billAgg = (await Query.AsNoTracking()
                .Where(b => b.BillDate >= start && b.BillDate < end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    BillDiscounts = g.Sum(b => b.ValueDiscount),
                    Net = g.Sum(b => b.TheNet),
                    Collected = g.Sum(b => b.PaidUp),
                    Outstanding = g.Sum(b => b.TheRest)
                })
                .ToListAsync()).FirstOrDefault();

            var lineAgg = (await _context.BillItems.AsNoTracking()
                .Where(i => i.Bill.BillDate >= start && i.Bill.BillDate < end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Gross = g.Sum(i => i.Total),
                    ItemDiscounts = g.Sum(i => i.DiscountAmount)
                })
                .ToListAsync()).FirstOrDefault();

            return new SalesTotalsRow
            {
                BillsCount = billAgg?.Count ?? 0,
                BillDiscounts = billAgg?.BillDiscounts ?? 0,
                Net = billAgg?.Net ?? 0,
                Collected = billAgg?.Collected ?? 0,
                Outstanding = billAgg?.Outstanding ?? 0,
                GrossTotal = lineAgg?.Gross ?? 0,
                ItemsDiscounts = lineAgg?.ItemDiscounts ?? 0
            };
        }

        public async Task<List<BillSummaryRow>> GetBillSummariesAsync(DateTime from, DateTime to)
        {
            var (start, end) = Range(from, to);

            return await Query.AsNoTracking()
                .Where(b => b.BillDate >= start && b.BillDate < end)
                .OrderByDescending(b => b.BillDate).ThenByDescending(b => b.Id)
                .Select(b => new BillSummaryRow
                {
                    Id = b.Id,
                    BillDate = b.BillDate,
                    ClientName = b.Client.Name,
                    ItemsCount = b.Items.Count(),
                    GrossTotal = b.Items.Sum(i => i.Total),
                    TotalDiscount = b.Items.Sum(i => i.DiscountAmount) + b.ValueDiscount,
                    TheNet = b.TheNet,
                    PaidUp = b.PaidUp,
                    TheRest = b.TheRest
                })
                .ToListAsync();
        }

        // Revenue = Balance الصنف بعد خصمه، ناقص نصيبه من خصم الفاتورة العام (بالتناسب)
        public async Task<List<ItemSalesRow>> GetTopItemsAsync(DateTime from, DateTime to, int take)
        {
            var (start, end) = Range(from, to);

            return await _context.BillItems.AsNoTracking()
                .Where(i => i.Bill.BillDate >= start && i.Bill.BillDate < end)
                .GroupBy(i => i.ItemId)
                .Select(g => new ItemSalesRow
                {
                    ItemName = g.Max(i => i.ItemName) ?? string.Empty,
                    UnitName = g.Max(i => i.UnitName) ?? string.Empty,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Bill.BillsTotal == 0
                        ? 0m
                        : i.Balance * i.Bill.TheNet / i.Bill.BillsTotal)
                })
                .OrderByDescending(r => r.Revenue)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<ClientSalesRow>> GetTopClientsAsync(DateTime from, DateTime to, int take)
        {
            var (start, end) = Range(from, to);

            return await Query.AsNoTracking()
                .Where(b => b.BillDate >= start && b.BillDate < end)
                .GroupBy(b => b.ClientId)
                .Select(g => new ClientSalesRow
                {
                    ClientName = g.Max(b => b.Client.Name) ?? string.Empty,
                    BillsCount = g.Count(),
                    TotalNet = g.Sum(b => b.TheNet)
                })
                .OrderByDescending(r => r.TotalNet)
                .Take(take)
                .ToListAsync();
        }
        // IgnoreQueryFilters: الدفعة تتحسب حتى لو فاتورتها اتحذفت (Soft Delete)، عشان نفس الـ Session متتسجلش تاني
        public async Task<bool> StripeSessionExistsAsync(string stripeSessionId) => await _context.Payments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(p => p.StripeSessionId == stripeSessionId);



        public async Task<Bill?> GetByStripePaymentIntentAsync(string paymentIntentId)
            => await Query.Include(b => b.Client)
                          .Include(b => b.Payments)
                          .AsSplitQuery()
                          .FirstOrDefaultAsync(b => b.Payments.Any(p => p.StripePaymentIntentId == paymentIntentId));
    }
}
