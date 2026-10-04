using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BillsSystem.Web.Extensions;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private const int PageSize = 20;

        private readonly IBillService _billService;
        private readonly IClientService _clientService;
        private readonly IItemService _itemService;
        private readonly IReminderService _reminderService;
        private readonly IConfiguration _configuration;

        public BillsController(IBillService billService, IClientService clientService, IItemService itemService,
            IReminderService reminderService, IConfiguration configuration)
        {
            _billService = billService;
            _clientService = clientService;
            _itemService = itemService;
            _reminderService = reminderService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var result = await _billService.GetPagedAsync(search, page, PageSize);
            ViewBag.Search = search;
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var bill = await _billService.GetByIdAsync(id);
            if (bill == null) return NotFound();
            return View(bill);
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new BillFormViewModel { BillDate = AppClock.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillFormViewModel model)
        {
            if (ModelState.HasRealBindingErrors())
            {
                ModelState.AddModelError(string.Empty, "Some values are invalid. Please check the numbers you entered");
                var its = await PopulateDropdownsAsync();
                FillItemDisplayNames(model, its);
                return View(model);
            }


            var input = new BillInput
            {
                SubmissionId = model.SubmissionId,
                BillDate = model.BillDate ?? default,
                ClientId = model.ClientId,
                DiscountType = model.DiscountType,
                PercentageDiscount = model.PercentageDiscount,
                ValueDiscount = model.ValueDiscount,
                PaidUp = model.PaidUp,
                DueDate = model.DueDate,
                Items = model.Items.Select(i => new BillItemInput
                {
                    ItemId = i.ItemId,
                    Quantity = i.Quantity,
                    SellingPrice = i.SellingPrice,
                    DiscountType = i.DiscountType,
                    Discount = i.Discount
                }).ToList()
            };

            var result = await _billService.CreateAsync(input);

            if (!result.Success)
            {
                if (result.BillDateError != null) ModelState.AddModelError(nameof(model.BillDate), result.BillDateError);
                if (result.ClientError != null) ModelState.AddModelError(nameof(model.ClientId), result.ClientError);
                if (result.ItemsError != null) ModelState.AddModelError(nameof(model.Items), result.ItemsError);
                if (result.PercentageDiscountError != null) ModelState.AddModelError(nameof(model.PercentageDiscount), result.PercentageDiscountError);
                if (result.PaidUpError != null) ModelState.AddModelError(nameof(model.PaidUp), result.PaidUpError);
                if (result.ValueDiscountError != null) ModelState.AddModelError(nameof(model.ValueDiscount), result.ValueDiscountError);
                if (result.DueDateError != null) ModelState.AddModelError(nameof(model.DueDate), result.DueDateError);
                if (result.GeneralError != null) ModelState.AddModelError(string.Empty, result.GeneralError);

                foreach (var rowError in result.ItemRowErrors)
                {
                    if (rowError.ItemError != null) ModelState.AddModelError($"Items[{rowError.Index}].ItemId", rowError.ItemError);
                    if (rowError.QuantityError != null) ModelState.AddModelError($"Items[{rowError.Index}].Quantity", rowError.QuantityError);
                    if (rowError.SellingPriceError != null) ModelState.AddModelError($"Items[{rowError.Index}].SellingPrice", rowError.SellingPriceError);
                    if (rowError.DiscountError != null) ModelState.AddModelError($"Items[{rowError.Index}].Discount", rowError.DiscountError);
                }

                var items = await PopulateDropdownsAsync();
                FillItemDisplayNames(model, items);
                return View(model);
            }

            TempData["SuccessMessage"] = result.AlreadySaved
                ? "This invoice was already saved. Your latest changes were NOT applied"
                : "Sales invoice created successfully";
            return RedirectToAction(nameof(Details), new { id = result.BillId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _billService.DeleteAsync(id);
            TempData["SuccessMessage"] = success ? "Bill deleted successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayment(int id, decimal amount, DateTime? paymentDate, string? notes)
        {
            var (success, error) = await _billService.AddPaymentAsync(id, amount, paymentDate, notes);
            TempData["SuccessMessage"] = success ? "Payment added successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VoidPayment(int id, int paymentId, string? reason)
        {
            var (success, error) = await _billService.VoidPaymentAsync(id, paymentId, reason);
            TempData["SuccessMessage"] = success ? "Payment voided successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Details), new { id });
        }

        // بعت تذكير للعميل دلوقتي (من صفحة الفاتورة)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReminder(int id)
        {
            var (success, error) = await _reminderService.SendNowAsync(id);
            TempData["SuccessMessage"] = success ? "Reminder email sent to the client" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Details), new { id });
        }

        // تحديد ميعاد السداد لفاتورة عليها متبقي ومالهاش ميعاد (مثلًا بعد إلغاء دفعة على فاتورة كانت مدفوعة)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDueDate(int id, DateTime dueDate)
        {
            var (success, error) = await _billService.SetDueDateAsync(id, dueDate);
            TempData["SuccessMessage"] = success ? "Due date saved" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------- AJAX: بيتنادى لما اليوزر يختار صنف عشان يجيب سعره ووحدته ومخزونه ----------
        [HttpGet]
        public async Task<JsonResult> GetItemDetails(int itemId)
        {
            var item = await _itemService.GetByIdAsync(itemId);
            if (item == null) return Json(null);

            return Json(new
            {
                itemCode = item.Id,
                itemName = DisplayName(item),
                unitName = item.Unit.Name,
                sellingPrice = item.SellingPrice,
                stock = item.QuantityInStock
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayWithStripe(int id, decimal amount)
        {
            // App:PublicBaseUrl لو متظبط وصحيح (https) بنبني اللينكات منه، غير كده بنرجع لـ Host الطلب
            var configured = _configuration["App:PublicBaseUrl"]?.Trim().TrimEnd('/');
            var baseUrl = Uri.TryCreate(configured, UriKind.Absolute, out var parsedBase) && parsedBase.Scheme == Uri.UriSchemeHttps
                ? configured
                : null;
            var successPath = Url.Action(nameof(StripeSuccess), "Bills", new { id })!;
            var cancelPath = Url.Action(nameof(Details), "Bills", new { id })!;
            var successUrl = baseUrl == null ? $"{Request.Scheme}://{Request.Host}{successPath}" : baseUrl + successPath;
            var cancelUrl = baseUrl == null ? $"{Request.Scheme}://{Request.Host}{cancelPath}" : baseUrl + cancelPath;

            var (success, error, url) = await _billService.CreateStripeCheckoutAsync(id, amount, successUrl, cancelUrl);
            if (!success)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Details), new { id });
            }

            return Redirect(url!);
        }

        public async Task<IActionResult> StripeSuccess(int id, [FromQuery(Name = "session_id")] string? sessionId)
        {
            // الـ Webhook هو اللي بيسجل الدفعة؛ مبنقولش "received" إلا لو اتسجلت فعلًا
            if (!string.IsNullOrWhiteSpace(sessionId) && await _billService.IsStripePaymentRecordedAsync(id, sessionId))
                TempData["SuccessMessage"] = "Stripe payment received and recorded";
            else
                TempData["SuccessMessage"] = "Back from Stripe. If you completed the payment it will appear here within a few seconds. Refresh to check";

            return RedirectToAction(nameof(Details), new { id });
        }


        // "Company / Type / Item" عشان صنفين بنفس الاسم ميتلخبطوش
        private static string DisplayName(Item item)
            => $"{item.ItemType.Company.Name} / {item.ItemType.Name} / {item.Name}";

        private async Task<List<Item>> PopulateDropdownsAsync()
        {
            var clients = await _clientService.GetAllAsync();
            ViewBag.Clients = new SelectList(clients, "Id", "Name");

            var items = (await _itemService.GetAllAsync()).ToList();
            ViewBag.Items = new SelectList(
                items.Select(i => new { i.Id, Text = DisplayName(i) }).OrderBy(x => x.Text),
                "Id", "Text");
            return items;
        }

        // من الـ List اللي اتحمّلت خلاص (مفيش Query لكل صف)
        private static void FillItemDisplayNames(BillFormViewModel model, List<Item> items)
        {
            var byId = items.ToDictionary(i => i.Id);
            foreach (var row in model.Items)
            {
                if (byId.TryGetValue(row.ItemId, out var item))
                {
                    row.ItemName = DisplayName(item);
                    row.UnitName = item.Unit.Name;
                }
            }
        }
    }
}
