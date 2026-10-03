using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace BillsSystem.Web.Controllers
{
    [Route("webhooks/stripe")]
    [AllowAnonymous]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IBillService _billService;
        private readonly StripeSettings _settings;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(IBillService billService, IOptions<StripeSettings> settings,
            ILogger<StripeWebhookController> logger)
        {
            _billService = billService;
            _settings = settings.Value;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
       json, Request.Headers["Stripe-Signature"], _settings.WebhookSecret,
       throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed");
                return BadRequest();
            }

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;
                if (session != null && session.PaymentStatus == "paid"
       && int.TryParse(session.Metadata?.GetValueOrDefault("billId"), out var billId))
                {
                    var amount = (session.AmountTotal ?? 0) / 100m;
                    var (success, error) = await _billService.ConfirmStripePaymentAsync(
                        session.Id, session.PaymentIntentId, amount, billId);

                    if (!success)
                    {
                        _logger.LogError("Failed to confirm Stripe payment for session {SessionId}: {Error}", session.Id, error);
                        return StatusCode(StatusCodes.Status500InternalServerError);   // Stripe هتعيد المحاولة
                    }
                }
                return Ok();
            }

            // Ensure all code paths return an IActionResult
            return Ok();
        }
    }
}