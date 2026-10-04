using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.Common;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace BillsSystem.Infrastructure.Payments
{
    public class StripeCheckoutService : IStripeCheckoutService
    {
        public StripeCheckoutService(IOptions<StripeSettings> settings)
        {
            // بيتسجل مرة واحدة لكل Request؛ الـ Secret Key بتاعك بيتاخد من appsettings/user-secrets
            StripeConfiguration.ApiKey = settings.Value.SecretKey;
            _currency = settings.Value.Currency;
        }

        private readonly string _currency;

        public async Task<(string SessionId, string Url)> CreateCheckoutSessionAsync(
            int billId, decimal amount, string successUrl, string cancelUrl)
        {
            if (string.IsNullOrWhiteSpace(StripeConfiguration.ApiKey))
                throw new InvalidOperationException("Stripe:SecretKey is not configured");

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = _currency,
                            UnitAmount = StripeMoney.ToMinorUnits(amount, _currency), // Stripe بياخد المبلغ بالـ Cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Bills System — Invoice #{billId}"
                            }
                        }
                    }
                },
                Metadata = new Dictionary<string, string> { { "billId", billId.ToString() } },
                SuccessUrl = successUrl + (successUrl.Contains('?') ? "&" : "?") + "session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = cancelUrl
            };

            var session = await new SessionService().CreateAsync(options);
            return (session.Id, session.Url);
        }
    }
}
