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
    

        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Billing";

        public int TimeoutSeconds { get; set; } = 15;

        // Auto = حسب البورت (587/2525 → StartTls، 465 → SslOnConnect). أو حدده: None / StartTls / StartTlsWhenAvailable / SslOnConnect
        public string Security { get; set; } = "Auto";
    }
}
