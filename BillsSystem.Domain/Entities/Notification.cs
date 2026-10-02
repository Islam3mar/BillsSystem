using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Domain.Entities
{
    // سجل الأحداث (Activity Feed). حالة القراءة عامة لكل الأدمنز دلوقتي.
    // لو بقى فيه أكتر من أدمن وعايزين حالة قراءة لكل واحد: هنضيف جدول NotificationReads (UserId + NotificationId)
    public class Notification : BaseEntity
    {
        public NotificationType Type { get; set; }
        public NotificationAction Action { get; set; }
        public string Message { get; set; } = null!;

        // Id الكيان المرتبط (لو لسه موجود) عشان اللينك
        public int? EntityId { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
