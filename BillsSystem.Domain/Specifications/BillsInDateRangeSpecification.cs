using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Specifications
{
    // بيرجع كل الفواتير اللي تاريخها بين From و To (شامل الطرفين)
    public class BillsInDateRangeSpecification : BaseSpecification<Bill>
    {
        public BillsInDateRangeSpecification(DateTime from, DateTime to)
            : base(b => b.BillDate.Date >= from.Date && b.BillDate.Date <= to.Date)
        {
            AddInclude("Client");
            AddInclude("Items.Item.Unit");
            ApplyOrderByDescending(b => b.BillDate);
        }
    }
}
