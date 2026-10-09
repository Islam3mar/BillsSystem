using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Specifications
{
    // صفحة من الفواتير + بحث برقم الفاتورة أو اسم العميل أو اسم الصنف + فلتر بنطاق تاريخ
    public class BillsPagedSpecification : BaseSpecification<Bill>
    {
        public BillsPagedSpecification(string? search, int page, int pageSize, bool includeItems = false,
            DateTime? from = null, DateTime? to = null)
        {
            var hasTerm = !string.IsNullOrWhiteSpace(search);
            var term = hasTerm ? search!.Trim() : string.Empty;
            var hasId = int.TryParse(term, out var id);

            // لو اليوزر عكس التاريخين نرتبهم بدل ما نرجّع نتيجة فاضية
            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                (from, to) = (to, from);

            var hasFrom = from.HasValue;
            var hasTo = to.HasValue;
            var fromDate = from?.Date ?? DateTime.MinValue;
            var toExclusive = (to?.Date ?? DateTime.MinValue).AddDays(1);   // شامل يوم "To" بالكامل

            if (hasTerm || hasFrom || hasTo)
            {
                AddCriteria(b =>
                    (!hasTerm
                        || (hasId && b.Id == id)
                        || b.Client.Name.Contains(term)
                        || b.Items.Any(i => i.ItemName.Contains(term)))
                    && (!hasFrom || b.BillDate >= fromDate)
                    && (!hasTo || b.BillDate < toExclusive));
            }

            AddInclude("Client");
            if (includeItems) AddInclude("Items");   // أسماء الأصناف في قائمة الفواتير
            ApplyOrderByDescending(b => b.BillDate);
            ApplyThenByDescending(b => b.Id);
            ApplyPaging((Math.Max(page, 1) - 1) * pageSize, pageSize);
        }
    }
}
