using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService) => _companyService = companyService;

        public async Task<IActionResult> Index()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return View(companies);
        }

        public IActionResult Create() => View(new CompanyFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanyFormViewModel model)
        {
            var result = await _companyService.CreateCompanyAsync(new CompanyInput { Name = model.Name, Notes = model.Notes });

            if (!result.Success)
            {
                ModelState.AddModelError(nameof(model.Name), result.NameError!);
                return View(model);
            }

            TempData["SuccessMessage"] = "Company added successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null) return NotFound();

            return View(new CompanyFormViewModel { Id = company.Id, Name = company.Name, Notes = company.Notes });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyFormViewModel model)
        {
            var result = await _companyService.UpdateCompanyAsync(model.Id, new CompanyInput { Name = model.Name, Notes = model.Notes });

            if (!result.Success)
            {
                ModelState.AddModelError(nameof(model.Name), result.NameError!);
                return View(model);
            }

            TempData["SuccessMessage"] = "Company updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _companyService.DeleteCompanyAsync(id);
            TempData["SuccessMessage"] = success ? "Company deleted successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Index));
        }
    }
}
