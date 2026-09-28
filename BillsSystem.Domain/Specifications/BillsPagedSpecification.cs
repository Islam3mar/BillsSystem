using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Specifications
{
    // صفحة من الفواتير + بحث برقم الفاتورة أو اسم العميل
    public class BillsPagedSpecification : BaseSpecification<Bill>
    {
        public BillsPagedSpecification(string? search, int page, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                if (int.TryParse(term, out var id))
                    AddCriteria(b => b.Id == id || b.Client.Name.Contains(term));
                else
                    AddCriteria(b => b.Client.Name.Contains(term));
            }

            AddInclude("Client");
            ApplyOrderByDescending(b => b.BillDate);
            ApplyThenByDescending(b => b.Id);
            ApplyPaging((Math.Max(page, 1) - 1) * pageSize, pageSize);
        }
    }
}
