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
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService) => _dashboardService = dashboardService;

        public async Task<IActionResult> Index()
        {
            var d = await _dashboardService.GetAsync();

            var vm = new HomeDashboardViewModel
            {
                TotalClients = d.TotalClients,
                TotalItems = d.TotalItems,
                TotalCompanies = d.TotalCompanies,
                TotalBillsAllTime = d.TotalBillsAllTime,
                OutstandingAllTime = d.OutstandingAllTime,
                MonthNetSales = d.MonthNetSales,
                MonthBillsCount = d.MonthBillsCount,
                RecentBills = d.RecentBills.Select(b => new RecentBillItem
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

        [AllowAnonymous]   // صفحة الخطأ لازم تفتح حتى لو الجلسة انتهت
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
