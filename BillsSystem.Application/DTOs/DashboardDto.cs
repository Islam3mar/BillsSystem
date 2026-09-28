using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.DTOs
{
    public class DashboardDto
    {
        public int TotalClients { get; set; }
        public int TotalItems { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalBillsAllTime { get; set; }
        public decimal OutstandingAllTime { get; set; }
        public decimal MonthNetSales { get; set; }
        public int MonthBillsCount { get; set; }
        public List<Bill> RecentBills { get; set; } = new();   // Client متحمّل
    }
}
