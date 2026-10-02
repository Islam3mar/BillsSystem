using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BillsSystem.Web.ViewModels
{
    // شكل الإشعار اللي بيروح للـ View وللـ JSON (الأيقونة واللون واللينك بتتحدد هنا مش في الداتابيز)
    public class NotificationViewModel
    {
        public int Id { get; init; }
        public string Message { get; init; } = "";
        public string Icon { get; init; } = "bi-bell";
        public string Tone { get; init; } = "info";          // success | info | danger | warning
        public string? Url { get; init; }
        public bool IsRead { get; init; }
        public string TimeAgo { get; init; } = "";
        public string FullTime { get; init; } = "";

        public static NotificationViewModel From(Notification n, IUrlHelper url) => new()
        {
            Id = n.Id,
            Message = n.Message,
            Icon = IconFor(n.Type),
            Tone = ToneFor(n.Action),
            Url = UrlFor(n, url),
            IsRead = n.IsRead,
            TimeAgo = FormatTimeAgo(n.CreatedAt),
            FullTime = n.CreatedAt.ToString("yyyy-MM-dd HH:mm")
        };

        private static string IconFor(NotificationType t) => t switch
        {
            NotificationType.Company => "bi-pencil-square",
            NotificationType.Unit => "bi-grid-3x3-gap",
            NotificationType.ItemType => "bi-clock-history",
            NotificationType.Category => "bi-layers",
            NotificationType.Item => "bi-box-seam",
            NotificationType.Client => "bi-people",
            NotificationType.Bill => "bi-receipt",
            NotificationType.Payment => "bi-cash-coin",
            _ => "bi-bell"
        };

        private static string ToneFor(NotificationAction a) => a switch
        {
            NotificationAction.Created => "success",
            NotificationAction.Updated => "info",
            NotificationAction.Deleted => "danger",
            NotificationAction.Alert => "warning",
            _ => "info"
        };

        private static string? UrlFor(Notification n, IUrlHelper url)
        {
            // الفاتورة والدفعة بيروحوا لتفاصيل الفاتورة. باقي الكيانات لصفحة القائمة
            if (n.Type is NotificationType.Bill or NotificationType.Payment)
            {
                // فاتورة اتحذفت مفيش ليها صفحة
                if (n.Type == NotificationType.Bill && n.Action == NotificationAction.Deleted) return null;
                return n.EntityId.HasValue
                    ? url.Action("Details", "Bills", new { id = n.EntityId.Value })
                    : url.Action("Index", "Bills");
            }

            if (n.Action == NotificationAction.Deleted) return null;

            var controller = n.Type switch
            {
                NotificationType.Company => "Companies",
                NotificationType.Unit => "Units",
                NotificationType.ItemType => "Types",
                NotificationType.Category => "Categories",
                NotificationType.Item => "Items",
                NotificationType.Client => "Clients",
                _ => null
            };
            return controller == null ? null : url.Action("Index", controller);
        }

        // CreatedAt متخزن بتوقيت مصر (AppClock) فنقارنه بنفس التوقيت
        private static string FormatTimeAgo(DateTime createdAt)
        {
            var diff = AppClock.Now - createdAt;
            if (diff.TotalSeconds < 60) return "just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} h ago";
            if (diff.TotalDays < 2) return "yesterday";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
            return createdAt.ToString("yyyy-MM-dd");
        }
    }
}
