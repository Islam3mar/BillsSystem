using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Specifications
{
    // صفحة من الأصناف + بحث باسم الصنف أو النوع أو الشركة أو الوحدة أو الملاحظات
    public class ItemsPagedSpecification : BaseSpecification<Item>
    {
        public ItemsPagedSpecification(string? search, int page, int pageSize)
        {
            var term = string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim();

            if (term.Length > 0)
            {
                var hasId = int.TryParse(term, out var id);

                AddCriteria(i =>
                    (hasId && i.Id == id)
                    || i.Name.Contains(term)
                    || i.ItemType.Name.Contains(term)
                    || i.ItemType.Company.Name.Contains(term)
                    || i.Unit.Name.Contains(term)
                    || (i.Notes != null && i.Notes.Contains(term)));
            }

            AddInclude("ItemType.Company");   // الشركة والنوع بيتعرضوا في الجدول
            AddInclude("Unit");
            ApplyOrderBy(i => i.Id);
            ApplyPaging((Math.Max(page, 1) - 1) * pageSize, pageSize);
        }
    }
}
