using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private readonly IBillService _billService;
        private readonly IClientService _clientService;
        private readonly IItemService _itemService;

        public BillsController(IBillService billService, IClientService clientService, IItemService itemService)
        {
            _billService = billService;
            _clientService = clientService;
            _itemService = itemService;
        }

        public async Task<IActionResult> Index()
        {
            var bills = await _billService.GetAllAsync();
            return View(bills);
        }

        public async Task<IActionResult> Details(int id)
        {
            var bill = await _billService.GetByIdAsync(id);
            if (bill == null) return NotFound();
            return View(bill);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new BillFormViewModel { BillDate = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillFormViewModel model)
        {
            var input = new BillInput
            {
                BillDate = model.BillDate ?? default,
                ClientId = model.ClientId,
                DiscountType = model.DiscountType,
                PercentageDiscount = model.PercentageDiscount,
                ValueDiscount = model.ValueDiscount,
                PaidUp = model.PaidUp,
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

                foreach (var rowError in result.ItemRowErrors)
                {
                    if (rowError.ItemError != null) ModelState.AddModelError($"Items[{rowError.Index}].ItemId", rowError.ItemError);
                    if (rowError.QuantityError != null) ModelState.AddModelError($"Items[{rowError.Index}].Quantity", rowError.QuantityError);
                    if (rowError.SellingPriceError != null) ModelState.AddModelError($"Items[{rowError.Index}].SellingPrice", rowError.SellingPriceError);
                    if (rowError.DiscountError != null) ModelState.AddModelError($"Items[{rowError.Index}].Discount", rowError.DiscountError);
                }

                await PopulateDropdownsAsync();
                await FillItemDisplayNamesAsync(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "Sales invoice created successfully";
            return RedirectToAction(nameof(Details), new { id = result.Bill!.Id });
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

        // ---------- AJAX: بيتنادى لما اليوزر يختار صنف عشان يجيب سعره ووحدته ----------
        [HttpGet]
        public async Task<JsonResult> GetItemDetails(int itemId)
        {
            var item = await _itemService.GetByIdAsync(itemId);
            if (item == null) return Json(null);

            return Json(new
            {
                itemCode = item.Id,
                itemName = item.Name,
                unitName = item.Unit.Name,
                sellingPrice = item.SellingPrice
            });
        }

        private async Task PopulateDropdownsAsync()
        {
            var clients = await _clientService.GetAllAsync();
            ViewBag.Clients = new SelectList(clients, "Id", "Name");

            var items = await _itemService.GetAllAsync();
            ViewBag.Items = new SelectList(items, "Id", "Name");
        }

        private async Task FillItemDisplayNamesAsync(BillFormViewModel model)
        {
            foreach (var row in model.Items)
            {
                var item = await _itemService.GetByIdAsync(row.ItemId);
                row.ItemName = item?.Name;
                row.UnitName = item?.Unit.Name;
            }
        }
    }
}
