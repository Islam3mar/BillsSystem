using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.Extensions;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BillsSystem.Web.Extensions;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ICompanyService _companyService;
        private readonly IItemTypeService _itemTypeService;
        private readonly IUnitService _unitService;

        public ItemsController(
            IItemService itemService,
            ICompanyService companyService,
            IItemTypeService itemTypeService,
            IUnitService unitService)
        {
            _itemService = itemService;
            _companyService = companyService;
            _itemTypeService = itemTypeService;
            _unitService = unitService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _itemService.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCompaniesAsync();
            await PopulateUnitsAsync();
            return View(new ItemFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemFormViewModel model)
        {
            if (ModelState.HasRealBindingErrors())
                return await ReturnFormWithBindingErrorAsync(model);


            var result = await _itemService.CreateAsync(new ItemInput
            {
                ItemTypeId = model.ItemTypeId,
                UnitId = model.UnitId,
                Name = model.Name,
                SellingPrice = model.SellingPrice,
                BuyingPrice = model.BuyingPrice,
                QuantityInStock = model.QuantityInStock,
                Notes = model.Notes
            });

            if (!result.Success)
            {
                if (result.TypeError != null) ModelState.AddModelError(nameof(model.ItemTypeId), result.TypeError);
                if (result.UnitError != null) ModelState.AddModelError(nameof(model.UnitId), result.UnitError);
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                if (result.SellingPriceError != null) ModelState.AddModelError(nameof(model.SellingPrice), result.SellingPriceError);
                if (result.BuyingPriceError != null) ModelState.AddModelError(nameof(model.BuyingPrice), result.BuyingPriceError);
                if (result.StockError != null) ModelState.AddModelError(nameof(model.QuantityInStock), result.StockError);

                await PopulateCompaniesAsync(model.CompanyId);
                await PopulateTypesAsync(model.CompanyId, model.ItemTypeId);
                await PopulateUnitsAsync(model.UnitId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Item added successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null) return NotFound();

            var model = new ItemFormViewModel
            {
                Id = item.Id,
                ItemTypeId = item.ItemTypeId,
                CompanyId = item.ItemType.CompanyId,
                UnitId = item.UnitId,
                Name = item.Name,
                SellingPrice = item.SellingPrice,
                BuyingPrice = item.BuyingPrice,
                QuantityInStock = item.QuantityInStock,
                Notes = item.Notes,
                RowVersion = item.RowVersion,
            };

            await PopulateCompaniesAsync(model.CompanyId);
            await PopulateTypesAsync(model.CompanyId, model.ItemTypeId);
            await PopulateUnitsAsync(model.UnitId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemFormViewModel model)
        {
            if (ModelState.HasRealBindingErrors())
                return await ReturnFormWithBindingErrorAsync(model);


            var result = await _itemService.UpdateAsync(model.Id, new ItemInput
            {
                ItemTypeId = model.ItemTypeId,
                UnitId = model.UnitId,
                Name = model.Name,
                SellingPrice = model.SellingPrice,
                BuyingPrice = model.BuyingPrice,
                QuantityInStock = model.QuantityInStock,
                Notes = model.Notes,
                RowVersion = model.RowVersion,
            });

            if (!result.Success)
            {
                if (result.TypeError != null) ModelState.AddModelError(nameof(model.ItemTypeId), result.TypeError);
                if (result.UnitError != null) ModelState.AddModelError(nameof(model.UnitId), result.UnitError);
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                if (result.SellingPriceError != null) ModelState.AddModelError(nameof(model.SellingPrice), result.SellingPriceError);
                if (result.BuyingPriceError != null) ModelState.AddModelError(nameof(model.BuyingPrice), result.BuyingPriceError);
                if (result.StockError != null) ModelState.AddModelError(nameof(model.QuantityInStock), result.StockError);

                await PopulateCompaniesAsync(model.CompanyId);
                await PopulateTypesAsync(model.CompanyId, model.ItemTypeId);
                await PopulateUnitsAsync(model.UnitId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Item updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _itemService.DeleteAsync(id);
            TempData["SuccessMessage"] = success ? "Item deleted successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCompaniesAsync(int? selectedId = null)
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name", selectedId);
        }

        private async Task PopulateTypesAsync(int companyId, int? selectedId = null)
        {
            var types = companyId > 0 ? await _itemTypeService.GetByCompanyAsync(companyId) : Enumerable.Empty<Domain.Entities.ItemType>();
            ViewBag.Types = new SelectList(types, "Id", "Name", selectedId);
        }

        private async Task PopulateUnitsAsync(int? selectedId = null)
        {
            var units = await _unitService.GetAllUnitsAsync();
            ViewBag.Units = new SelectList(units, "Id", "Name", selectedId);
        }



        private async Task<IActionResult> ReturnFormWithBindingErrorAsync(ItemFormViewModel model)
        {
            ModelState.AddModelError(string.Empty, "Some values are invalid. Please check the numbers you entered");
            await PopulateCompaniesAsync(model.CompanyId);
            await PopulateTypesAsync(model.CompanyId, model.ItemTypeId);
            await PopulateUnitsAsync(model.UnitId);
            return View(model);
        }
    }
}
