using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    // بتتقرأ من appsettings.json -> "Reminders"
    public class ReminderSettings
    {
        public bool Enabled { get; set; } = true;

        // يبعت "تذكير قبل الاستحقاق" لما يفضل على الميعاد كام يوم أو أقل (0 = يوم الاستحقاق نفسه بس)
        public int DaysBeforeDue { get; set; } = 3;

        // بعد الاستحقاق: يكرر "تنبيه التأخير" كل كام يوم لحد ما العميل يسدد
        public int OverdueRepeatEveryDays { get; set; } = 3;

        // الـ Background Service بيفحص كل كام دقيقة (وبيفحص أول ما التطبيق يشتغل = Catch-up)
        public int CheckIntervalMinutes { get; set; } = 60;
        public int StartupDelaySeconds { get; set; } = 30;

        // حماية: أقصى عدد إيميلات في الدورة الواحدة
        public int MaxEmailsPerRun { get; set; } = 50;

        // إيميل سقف الدين: يتبعت لما الدين بعد الفاتورة يوصل للنسبة دي من الحد (أو لما فاتورة تترفض)
        public int CreditWarningPercent { get; set; } = 90;
        public int CreditEmailCooldownHours { get; set; } = 24;

        public string Currency { get; set; } = "جنيه";
    }
}
