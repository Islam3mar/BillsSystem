using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Reports
{
    public class SalesTotalsRow
    {
        public int BillsCount { get; set; }
        public decimal GrossTotal { get; set; }        // مجموع Total الأصناف قبل أي خصم
        public decimal ItemsDiscounts { get; set; }
        public decimal BillDiscounts { get; set; }
        public decimal TotalDiscounts => ItemsDiscounts + BillDiscounts;
        public decimal Net { get; set; }
        public decimal Collected { get; set; }
        public decimal Outstanding { get; set; }
    }

    public class BillSummaryRow
    {
        public int Id { get; set; }
        public DateTime BillDate { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TheNet { get; set; }
        public decimal PaidUp { get; set; }
        public decimal TheRest { get; set; }
    }

    public class ItemSalesRow
    {
        public string ItemName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ClientSalesRow
    {
        public string ClientName { get; set; } = string.Empty;
        public int BillsCount { get; set; }
        public decimal TotalNet { get; set; }
    }
}
