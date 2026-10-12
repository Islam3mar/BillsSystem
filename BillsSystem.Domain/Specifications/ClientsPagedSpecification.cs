using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Specifications
{
    // صفحة من العملاء + بحث بالرقم أو الاسم أو التليفون أو العنوان أو الإيميل
    public class ClientsPagedSpecification : BaseSpecification<Client>
    {
        public ClientsPagedSpecification(string? search, int page, int pageSize)
        {
            var term = string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim();

            if (term.Length > 0)
            {
                var hasId = int.TryParse(term, out var id);

                AddCriteria(c =>
                    (hasId && c.Id == id)
                    || c.Name.Contains(term)
                    || c.Phone.Contains(term)
                    || c.Address.Contains(term)
                    || (c.Email != null && c.Email.Contains(term)));
            }

            ApplyOrderBy(c => c.Id);   // نفس الترتيب اللي كان معروض قبل الـ Paging
            ApplyPaging((Math.Max(page, 1) - 1) * pageSize, pageSize);
        }
    }
}
