using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.Interfaces
{
    public interface IStripeCheckoutService
    {
        Task<(string SessionId, string Url)> CreateCheckoutSessionAsync(
            int billId, decimal amount, string successUrl, string cancelUrl);
    }
}
