using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Specifications
{
    // بيرجع كل الفواتير اللي تاريخها بين From و To (شامل الطرفين)
    // من غير .Date على العمود، عشان الـ Index على BillDate يشتغل
    public class BillsInDateRangeSpecification : BaseSpecification<Bill>
    {
        public BillsInDateRangeSpecification(DateTime from, DateTime to)
        {
            var start = from.Date;
            var endExclusive = to.Date.AddDays(1);
            AddCriteria(b => b.BillDate >= start && b.BillDate < endExclusive);
            AddInclude("Client");
            AddInclude("Items");
            ApplyOrderByDescending(b => b.BillDate);
            ApplyThenByDescending(b => b.Id);
        }
    }
}
