using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class TypesController : Controller
    {
        private readonly IItemTypeService _itemTypeService;
        private readonly ICompanyService _companyService;

        public TypesController(IItemTypeService itemTypeService, ICompanyService companyService)
        {
            _itemTypeService = itemTypeService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            var types = await _itemTypeService.GetAllAsync();
            return View(types);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCompaniesAsync();
            return View(new ItemTypeFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ItemTypeFormViewModel model)
        {
            var result = await _itemTypeService.CreateAsync(new ItemTypeInput
            {
                CompanyId = model.CompanyId,
                Name = model.Name,
                Notes = model.Notes
            });

            if (!result.Success)
            {
                if (result.CompanyError != null) ModelState.AddModelError(nameof(model.CompanyId), result.CompanyError);
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                await PopulateCompaniesAsync(model.CompanyId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Type added successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var itemType = await _itemTypeService.GetByIdAsync(id);
            if (itemType == null) return NotFound();

            await PopulateCompaniesAsync(itemType.CompanyId);
            return View(new ItemTypeFormViewModel
            {
                Id = itemType.Id,
                CompanyId = itemType.CompanyId,
                Name = itemType.Name,
                Notes = itemType.Notes
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ItemTypeFormViewModel model)
        {
            var result = await _itemTypeService.UpdateAsync(model.Id, new ItemTypeInput
            {
                CompanyId = model.CompanyId,
                Name = model.Name,
                Notes = model.Notes
            });

            if (!result.Success)
            {
                if (result.CompanyError != null) ModelState.AddModelError(nameof(model.CompanyId), result.CompanyError);
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                await PopulateCompaniesAsync(model.CompanyId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Type updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _itemTypeService.DeleteAsync(id);
            TempData["SuccessMessage"] = success ? "Type deleted successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Index));
        }

        // ---------- Cascading Dropdown: بيتنادى من شاشة الـ Category بالـ AJAX ----------
        [HttpGet]
        public async Task<JsonResult> GetByCompany(int companyId)
        {
            var types = await _itemTypeService.GetByCompanyAsync(companyId);
            return Json(types.Select(t => new { id = t.Id, name = t.Name }));
        }

        private async Task PopulateCompaniesAsync(int? selectedId = null)
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            ViewBag.Companies = new SelectList(companies, "Id", "Name", selectedId);
        }
    }
}
