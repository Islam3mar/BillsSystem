using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    // بتتقرأ من appsettings.json -> "Email"
    public class EmailSettings
    {
        // false = مفيش أي إيميل بيتبعت (الفواتير اللي عملاؤها ليهم إيميل بتستنى لحد ما تفعّله)
        public bool Enabled { get; set; }

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;     // STARTTLS (587 / 2525). الـ SSL الصريح على 465 مش مدعوم هنا

        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Billing";

        public int TimeoutSeconds { get; set; } = 15;
    }
}
