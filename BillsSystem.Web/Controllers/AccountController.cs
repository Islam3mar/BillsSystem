using System.Security.Claims;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BillsSystem.Web.Controllers
{
    [AllowAnonymous]   // ضروري مع الـ Fallback Policy عشان صفحة Login تفضل مفتوحة
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAuthService authService, ILogger<AccountController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("login")]   // الـ Policy معرّفة في Program.cs
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var isValid = await _authService.ValidateCredentialsAsync(model.Username, model.Password);
            if (!isValid)
            {
                _logger.LogWarning("Failed login attempt for username '{Username}' from {Ip}",
                    SafeForLog(model.Username), HttpContext.Connection.RemoteIpAddress);
                ModelState.AddModelError(string.Empty, "Invalid username or password");
                return View(model);
            }

            _logger.LogInformation("Successful login for '{Username}' from {Ip}",
                SafeForLog(model.Username), HttpContext.Connection.RemoteIpAddress);

            var claims = new List<Claim> { new(ClaimTypes.Name, model.Username) };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false });

            // IsLocalUrl: عشان محدش يبعتلك لينك يوديك لموقع تاني بعد الـ Login (Open Redirect)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // اسم المستخدم جاي من برّه: نشيل الأسطر الجديدة (عشان محدش يزوّر سطور في اللوج) ونقص الطول
        private static string SafeForLog(string? value)
        {
            var clean = (value ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ');
            return clean.Length > 50 ? clean[..50] : clean;
        }

    }
}
