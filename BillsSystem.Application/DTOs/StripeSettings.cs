using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string Currency { get; set; } = "usd";
    }
}
