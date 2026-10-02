using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BillsSystem.Application.Services
{
    public class NotificationService : INotificationService
    {
        private const int MaxMessageLength = 500;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task NotifyAsync(NotificationType type, NotificationAction action, string message, int? entityId = null)
        {
            try
            {
                if (message.Length > MaxMessageLength) message = message[..(MaxMessageLength - 1)] + "…";

                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    Type = type,
                    Action = action,
                    Message = message,
                    EntityId = entityId
                });
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // الإشعار تابع للعملية، مش مسموح يوقعها بعد ما اتحفظت
                _logger.LogError(ex, "Failed to save notification: {Message}", message);
            }
        }

        public async Task<IReadOnlyList<Notification>> GetRecentAsync(int take = 10)
            => await _unitOfWork.Notifications.GetRecentAsync(Math.Clamp(take, 1, 50));

        public async Task<int> GetUnreadCountAsync()
            => await _unitOfWork.Notifications.CountAsync(unreadOnly: true);

        public async Task<PagedResult<Notification>> GetPagedAsync(int page, int pageSize, bool unreadOnly)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var total = await _unitOfWork.Notifications.CountAsync(unreadOnly);
            var items = await _unitOfWork.Notifications.GetPageAsync(page, pageSize, unreadOnly);

            return new PagedResult<Notification> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
        }

        public async Task MarkAsReadAsync(int id)
            => await _unitOfWork.Notifications.MarkAsReadAsync(id, AppClock.Now);

        public async Task MarkAllAsReadAsync()
            => await _unitOfWork.Notifications.MarkAllAsReadAsync(AppClock.Now);
    }
}
