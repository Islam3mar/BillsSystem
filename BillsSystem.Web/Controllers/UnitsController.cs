using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class UnitsController : Controller
    {
        private readonly IUnitService _unitService;

        public UnitsController(IUnitService unitService) => _unitService = unitService;

        public async Task<IActionResult> Index()
        {
            var units = await _unitService.GetAllUnitsAsync();
            return View(units);
        }

        public IActionResult Create() => View(new UnitFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitFormViewModel model)
        {
            var result = await _unitService.CreateUnitAsync(new UnitInput { Name = model.Name, Notes = model.Notes });

            if (!result.Success)
            {
                ModelState.AddModelError(nameof(model.Name), result.NameError!);
                return View(model);
            }

            TempData["SuccessMessage"] = "Unit added successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _unitService.GetUnitByIdAsync(id);
            if (unit == null) return NotFound();

            return View(new UnitFormViewModel { Id = unit.Id, Name = unit.Name, Notes = unit.Notes });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UnitFormViewModel model)
        {
            var result = await _unitService.UpdateUnitAsync(model.Id, new UnitInput { Name = model.Name, Notes = model.Notes });

            if (!result.Success)
            {
                ModelState.AddModelError(nameof(model.Name), result.NameError!);
                return View(model);
            }

            TempData["SuccessMessage"] = "Unit updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _unitService.DeleteUnitAsync(id);
            TempData["SuccessMessage"] = success ? "Unit deleted successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Index));
        }
    }
}
