using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillsSystem.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private const int PageSize = 20;
        private const int DropdownSize = 10;

        private readonly INotificationService _notifications;

        public NotificationsController(INotificationService notifications) => _notifications = notifications;

        // الصفحة الكاملة (View all)
        public async Task<IActionResult> Index(int page = 1, bool unreadOnly = false)
        {
            var result = await _notifications.GetPagedAsync(page, PageSize, unreadOnly);
            ViewBag.UnreadOnly = unreadOnly;
            ViewBag.Unread = await _notifications.GetUnreadCountAsync();

            var vm = new PagedResult<NotificationViewModel>
            {
                Items = result.Items.Select(n => NotificationViewModel.From(n, Url)).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
            return View(vm);
        }

        // الـ Polling بيكلم ده كل 30 ثانية: العداد + آخر 10 إشعارات
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Summary()
        {
            var unread = await _notifications.GetUnreadCountAsync();
            var recent = await _notifications.GetRecentAsync(DropdownSize);

            return Json(new
            {
                unreadCount = unread,
                items = recent.Select(n => NotificationViewModel.From(n, Url))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notifications.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notifications.MarkAllAsReadAsync();
            return Ok();
        }
    }
}
