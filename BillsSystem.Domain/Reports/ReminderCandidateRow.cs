using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Enums;

namespace BillsSystem.Domain.Reports
{
    // صف خفيف (من غير Tracking) لفاتورة محتاجة تذكير + بيانات عميلها
    public class ReminderCandidateRow
    {
        public int BillId { get; set; }
        public DateTime BillDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TheNet { get; set; }
        public decimal TheRest { get; set; }

        public DateTime? LastReminderSentAt { get; set; }
        public ReminderType LastReminderType { get; set; }

        public DateTime? LastReminderAttemptAt { get; set; }

        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientEmail { get; set; }
    }
}
