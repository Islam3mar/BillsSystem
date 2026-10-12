using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService) => _clientService = clientService;

        private const int PageSize = 20;

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var result = await _clientService.GetPagedAsync(search, page, PageSize);

            // الصفحة فضيت (مثلًا بعد حذف آخر صف فيها) → نروح لآخر صفحة موجودة
            if (result.Items.Count == 0 && result.TotalCount > 0 && result.Page > 1)
                return RedirectToAction(nameof(Index), new { search, page = result.TotalPages });

            ViewBag.Search = search;
            return View(result);
        }

        public IActionResult Create() => View(new ClientFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientFormViewModel model)
        {
            var result = await _clientService.CreateAsync(new ClientInput
            {
                Name = model.Name,
                Phone = model.Phone,
                Address = model.Address,
                Email = model.Email,
                MaxCreditLimit = model.MaxCreditLimit
            });

            if (!result.Success)
            {
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                if (result.PhoneError != null) ModelState.AddModelError(nameof(model.Phone), result.PhoneError);
                if (result.AddressError != null) ModelState.AddModelError(nameof(model.Address), result.AddressError);
                if (result.EmailError != null) ModelState.AddModelError(nameof(model.Email), result.EmailError);
                if (result.MaxCreditLimitError != null) ModelState.AddModelError(nameof(model.MaxCreditLimit), result.MaxCreditLimitError);
                return View(model);
            }

            TempData["SuccessMessage"] = "Client added successfully";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();

            return View(new ClientFormViewModel
            {
                Id = client.Id,
                Name = client.Name,
                Phone = client.Phone,
                Address = client.Address,
                Email = client.Email,
                MaxCreditLimit = client.MaxCreditLimit
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientFormViewModel model)
        {
            var result = await _clientService.UpdateAsync(model.Id, new ClientInput
            {
                Name = model.Name,
                Phone = model.Phone,
                Address = model.Address,
                Email = model.Email,
                MaxCreditLimit = model.MaxCreditLimit
            });

            if (!result.Success)
            {
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                if (result.PhoneError != null) ModelState.AddModelError(nameof(model.Phone), result.PhoneError);
                if (result.AddressError != null) ModelState.AddModelError(nameof(model.Address), result.AddressError);
                if (result.EmailError != null) ModelState.AddModelError(nameof(model.Email), result.EmailError);
                if (result.MaxCreditLimitError != null) ModelState.AddModelError(nameof(model.MaxCreditLimit), result.MaxCreditLimitError);
                return View(model);
            }

            TempData["SuccessMessage"] = "Client updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? search = null, int page = 1)
        {
            var (success, error) = await _clientService.DeleteAsync(id);
            TempData["SuccessMessage"] = success ? "Client deleted successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Index), new { search, page });   // يفضل في نفس الصفحة والبحث
        }
    }
}
