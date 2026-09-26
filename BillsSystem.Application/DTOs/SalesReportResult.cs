using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    public class BillSummaryDto
    {
        public int Id { get; set; }
        public DateTime BillDate { get; set; }
        public string ClientName { get; set; } = null!;
        public int ItemsCount { get; set; }
        public decimal BillsTotal { get; set; }
        public decimal ValueDiscount { get; set; }
        public decimal TheNet { get; set; }
        public decimal PaidUp { get; set; }
        public decimal TheRest { get; set; }
    }

    public class ItemSalesDto
    {
        public string ItemName { get; set; } = null!;
        public string UnitName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ClientSalesDto
    {
        public string ClientName { get; set; } = null!;
        public int BillsCount { get; set; }
        public decimal TotalNet { get; set; }
    }
    //------------------
    public class SalesReportResult
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public int BillsCount { get; set; }
        public decimal TotalBillsAmount { get; set; }   // مجموع BillsTotal قبل الخصم العام
        public decimal TotalDiscounts { get; set; }       // مجموع ValueDiscount
        public decimal TotalNetSales { get; set; }        // مجموع TheNet
        public decimal TotalCollected { get; set; }       // مجموع PaidUp
        public decimal TotalOutstanding { get; set; }     // مجموع TheRest (ديون على العملاء)
        public decimal AverageBillValue { get; set; }     // TotalNetSales ÷ BillsCount

        public List<BillSummaryDto> Bills { get; set; } = new();
        public List<ItemSalesDto> TopSellingItems { get; set; } = new();
        public List<ClientSalesDto> TopClients { get; set; } = new();
    }
    //----------------
    public class SalesReportRequestResult
    {
        public bool Success { get; set; }
        public SalesReportResult? Report { get; set; }

        public string? FromDateError { get; set; }
        public string? ToDateError { get; set; }

        public bool HasErrors => FromDateError != null || ToDateError != null;
    }
}
