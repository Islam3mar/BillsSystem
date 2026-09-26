namespace BillsSystem.Web.ViewModels
{
    public class RecentBillItem
    {
        public int Id { get; set; }
        public DateTime BillDate { get; set; }
        public string ClientName { get; set; } = null!;
        public decimal TheNet { get; set; }
        public decimal TheRest { get; set; }
    }

    public class HomeDashboardViewModel
    {
        public int TotalClients { get; set; }
        public int TotalItems { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalBillsAllTime { get; set; }
        public decimal OutstandingAllTime { get; set; }

        public decimal MonthNetSales { get; set; }
        public int MonthBillsCount { get; set; }

        public List<RecentBillItem> RecentBills { get; set; } = new();
    }
}
