using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ICompanyService _companyService;
        private readonly IItemTypeService _itemTypeService;

        public CategoriesController(ICategoryService categoryService, ICompanyService companyService, IItemTypeService itemTypeService)
        {
            _categoryService = categoryService;
            _companyService = companyService;
            _itemTypeService = itemTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCompaniesAsync();
            return View(new CategoryFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryFormViewModel model)
        {
            var result = await _categoryService.CreateAsync(new CategoryInput
            {
                ItemTypeId = model.ItemTypeId,
                Name = model.Name,
                Notes = model.Notes
            });

            if (!result.Success)
            {
                if (result.TypeError != null) ModelState.AddModelError(nameof(model.ItemTypeId), result.TypeError);
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                await PopulateCompaniesAsync(model.CompanyId);
                await PopulateTypesAsync(model.CompanyId, model.ItemTypeId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Category added successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            var model = new CategoryFormViewModel
            {
                Id = category.Id,
                ItemTypeId = category.ItemTypeId,
                CompanyId = category.ItemType.CompanyId,
                Name = category.Name,
                Notes = category.Notes
            };

            await PopulateCompaniesAsync(model.CompanyId);
            await PopulateTypesAsync(model.CompanyId, model.ItemTypeId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryFormViewModel model)
        {
            var result = await _categoryService.UpdateAsync(model.Id, new CategoryInput
            {
                ItemTypeId = model.ItemTypeId,
                Name = model.Name,
                Notes = model.Notes
            });

            if (!result.Success)
            {
                if (result.TypeError != null) ModelState.AddModelError(nameof(model.ItemTypeId), result.TypeError);
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                await PopulateCompaniesAsync(model.CompanyId);
                await PopulateTypesAsync(model.CompanyId, model.ItemTypeId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Category updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _categoryService.DeleteAsync(id);
            TempData["SuccessMessage"] = success ? "Category deleted successfully" : null;
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
    }
}
