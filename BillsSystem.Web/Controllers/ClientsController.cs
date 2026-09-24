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

        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllAsync();
            return View(clients);
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
                Address = model.Address
            });

            if (!result.Success)
            {
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                if (result.PhoneError != null) ModelState.AddModelError(nameof(model.Phone), result.PhoneError);
                if (result.AddressError != null) ModelState.AddModelError(nameof(model.Address), result.AddressError);
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
                Address = client.Address
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
                Address = model.Address
            });

            if (!result.Success)
            {
                if (result.NameError != null) ModelState.AddModelError(nameof(model.Name), result.NameError);
                if (result.PhoneError != null) ModelState.AddModelError(nameof(model.Phone), result.PhoneError);
                if (result.AddressError != null) ModelState.AddModelError(nameof(model.Address), result.AddressError);
                return View(model);
            }

            TempData["SuccessMessage"] = "Client updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _clientService.DeleteAsync(id);
            TempData["SuccessMessage"] = success ? "Client deleted successfully" : null;
            TempData["ErrorMessage"] = success ? null : error;
            return RedirectToAction(nameof(Index));
        }
    }
}
