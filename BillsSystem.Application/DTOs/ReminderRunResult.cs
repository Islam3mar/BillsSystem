using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    public class ReminderRunResult
    {
        public int Due { get; set; }          // فواتير محتاجة تذكير دلوقتي
        public int EmailsSent { get; set; }
        public int InternalOnly { get; set; } // عميل من غير إيميل: تنبيه داخلي بس
        public int Skipped { get; set; }      // مؤجلة (الإيميل مقفول أو وصلنا للحد الأقصى في الدورة)
        public int Failed { get; set; }       // الإرسال فشل وهتتحاول تاني في الدورة الجاية
    }
}
