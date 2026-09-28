using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
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

        public async Task<int?> GetIdBySubmissionAsync(Guid submissionId)
            => await Query.AsNoTracking()
                          .Where(b => b.SubmissionId == submissionId)
                          .Select(b => (int?)b.Id)
                          .FirstOrDefaultAsync();

        public async Task<decimal> GetTotalOutstandingAsync()
            => await Query.AsNoTracking().SumAsync(b => b.TheRest);

        // ---------------- Reports (كلها SQL) ----------------
        private static (DateTime Start, DateTime EndExclusive) Range(DateTime from, DateTime to)
            => (from.Date, to.Date.AddDays(1));

        public async Task<SalesTotalsRow> GetSalesTotalsAsync(DateTime from, DateTime to)
        {
            var (start, end) = Range(from, to);

            var billAgg = await Query.AsNoTracking()
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
                .FirstOrDefaultAsync();

            var lineAgg = await _context.BillItems.AsNoTracking()
                .Where(i => i.Bill.BillDate >= start && i.Bill.BillDate < end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Gross = g.Sum(i => i.Total),
                    ItemDiscounts = g.Sum(i => i.DiscountAmount)
                })
                .FirstOrDefaultAsync();

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
    }
}
