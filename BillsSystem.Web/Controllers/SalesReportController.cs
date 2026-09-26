using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class SalesReportController : Controller
    {
        private readonly ISalesReportService _salesReportService;

        public SalesReportController(ISalesReportService salesReportService)
        {
            _salesReportService = salesReportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(SalesReportFilterViewModel filter)
        {
            // افتراضي: الشهر الحالي (أول يوم لحد النهاردة) لو المستخدم لسه ما فلترش
            filter.FromDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            filter.ToDate ??= DateTime.Today;

            var input = new SalesReportFilter
            {
                FromDate = filter.FromDate.Value,
                ToDate = filter.ToDate.Value
            };

            var result = await _salesReportService.GenerateAsync(input);

            if (!result.Success)
            {
                if (result.FromDateError != null) ModelState.AddModelError(nameof(filter.FromDate), result.FromDateError);
                if (result.ToDateError != null) ModelState.AddModelError(nameof(filter.ToDate), result.ToDateError);
            }
            else
            {
                ViewBag.Report = result.Report;
            }

            return View(filter);
        }
    }
}
