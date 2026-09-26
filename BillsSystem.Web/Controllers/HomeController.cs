using System.Diagnostics;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IBillService _billService;
        private readonly IClientService _clientService;
        private readonly IItemService _itemService;
        private readonly ICompanyService _companyService;
        private readonly ISalesReportService _salesReportService;

        public HomeController(
            IBillService billService,
            IClientService clientService,
            IItemService itemService,
            ICompanyService companyService,
            ISalesReportService salesReportService)
        {
            _billService = billService;
            _clientService = clientService;
            _itemService = itemService;
            _companyService = companyService;
            _salesReportService = salesReportService;
        }

        public async Task<IActionResult> Index()
        {
            var bills = (await _billService.GetAllAsync()).ToList();
            var clients = await _clientService.GetAllAsync();
            var items = await _itemService.GetAllAsync();
            var companies = await _companyService.GetAllCompaniesAsync();

            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var monthResult = await _salesReportService.GenerateAsync(new SalesReportFilter
            {
                FromDate = monthStart,
                ToDate = DateTime.Today
            });

            var vm = new HomeDashboardViewModel
            {
                TotalClients = clients.Count(),
                TotalItems = items.Count(),
                TotalCompanies = companies.Count(),
                TotalBillsAllTime = bills.Count,
                OutstandingAllTime = bills.Sum(b => b.TheRest),
                MonthNetSales = monthResult.Success ? monthResult.Report!.TotalNetSales : 0,
                MonthBillsCount = monthResult.Success ? monthResult.Report!.BillsCount : 0,
                RecentBills = bills
                    .OrderByDescending(b => b.BillDate).ThenByDescending(b => b.Id)
                    .Take(5)
                    .Select(b => new RecentBillItem
                    {
                        Id = b.Id,
                        BillDate = b.BillDate,
                        ClientName = b.Client.Name,
                        TheNet = b.TheNet,
                        TheRest = b.TheRest
                    }).ToList()
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
