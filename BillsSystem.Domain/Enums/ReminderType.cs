using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Enums
{
    // آخر نوع تذكير اتبعت على الفاتورة (None = لسه مفيش تذكير اتبعت)
    public enum ReminderType
    {
        None = 0,
        BeforeDue = 1,
        Overdue = 2
    }
}
