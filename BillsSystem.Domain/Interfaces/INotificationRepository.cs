using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> GetRecentAsync(int take);
        Task<IReadOnlyList<Notification>> GetPageAsync(int page, int pageSize, bool unreadOnly);
        Task<int> CountAsync(bool unreadOnly);
        Task<int> MarkAsReadAsync(int id, DateTime now);
        Task<int> MarkAllAsReadAsync(DateTime now);
    }
}
