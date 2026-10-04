using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Notification>> GetRecentAsync(int take) =>
            await Query.AsNoTracking()
                       .OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
                       .Take(take)
                       .ToListAsync();

        public async Task<IReadOnlyList<Notification>> GetPageAsync(int page, int pageSize, bool unreadOnly) =>
            await Filter(unreadOnly).AsNoTracking()
                       .OrderByDescending(n => n.CreatedAt).ThenByDescending(n => n.Id)
                       .Skip((page - 1) * pageSize).Take(pageSize)
                       .ToListAsync();

        public async Task<int> CountAsync(bool unreadOnly) => await Filter(unreadOnly).CountAsync();

        // ExecuteUpdate: UPDATE واحد في الداتابيز من غير ما نحمّل الصفوف في الذاكرة
        public async Task<int> MarkAsReadAsync(int id, DateTime now) =>
            await Query.Where(n => n.Id == id && !n.IsRead)
                       .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true).SetProperty(n => n.ReadAt, now));

        public async Task<int> MarkAllAsReadAsync(DateTime now) =>
            await Query.Where(n => !n.IsRead)
                       .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true).SetProperty(n => n.ReadAt, now));


        // DELETE واحد في الداتابيز من غير ما نحمّل الصفوف
        public async Task<int> DeleteOlderThanAsync(DateTime cutoff) =>
            await Query.Where(n => n.CreatedAt < cutoff).ExecuteDeleteAsync();

        private IQueryable<Notification> Filter(bool unreadOnly) =>
            unreadOnly ? Query.Where(n => !n.IsRead) : Query;
    }
}
