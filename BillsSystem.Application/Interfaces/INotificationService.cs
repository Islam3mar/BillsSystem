using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Application.Interfaces
{
    public interface INotificationService
    {
        // بتتنادى بعد ما العملية الأساسية تنجح. فشلها ما بيكسرش العملية (بيتسجل في الـ Log بس)
        Task NotifyAsync(NotificationType type, NotificationAction action, string message, int? entityId = null);

        Task<IReadOnlyList<Notification>> GetRecentAsync(int take = 10);
        Task<int> GetUnreadCountAsync();
        Task<PagedResult<Notification>> GetPagedAsync(int page, int pageSize, bool unreadOnly);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync();
    }
}
