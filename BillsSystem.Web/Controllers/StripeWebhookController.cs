using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using BillsSystem.Application.Common;

namespace BillsSystem.Web.Controllers
{
    [Route("webhooks/stripe")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]   // Stripe مش بيبعت AntiForgery Token، والحماية هنا بالـ Signature
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
            // من غير Secret مفيش طريقة نتأكد إن الطلب جاي من Stripe فعلًا، فنرفض بدل ما نثق في أي حد
            if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            {
                _logger.LogError("Stripe webhook called but Stripe:WebhookSecret is not configured");
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

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

            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                case "checkout.session.async_payment_succeeded":
                    return await HandleCheckoutPaidAsync(stripeEvent);

                case "charge.refunded":
                    return await HandleRefundAsync(stripeEvent);

                case "charge.dispute.created":
                    return await HandleDisputeAsync(stripeEvent);

                default:
                    return Ok();   // أحداث تانية مش محتاجينها
            }
        }

        private async Task<IActionResult> HandleCheckoutPaidAsync(Event stripeEvent)
        {
            // PaymentStatus مش "paid" = دفع لسه معلّق (async)، وهيجي async_payment_succeeded بعدين
            if (stripeEvent.Data.Object is not Session session || session.PaymentStatus != "paid")
                return Ok();

            if (!int.TryParse(session.Metadata?.GetValueOrDefault("billId"), out var billId))
                return Ok();

            // لو العملة مش عملة النظام منسجلش الدفعة بأرقام غلط، وإعادة المحاولة مش هتفيد
            var currency = session.Currency ?? _settings.Currency;
            if (!string.Equals(currency, _settings.Currency, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(
                    "Stripe session {SessionId} was paid in {Currency} but the system currency is {Expected}. Payment NOT recorded - check it in the Stripe Dashboard",
                    session.Id, currency, _settings.Currency);
                return Ok();
            }

            var amount = StripeMoney.FromMinorUnits(session.AmountTotal ?? 0, currency);
            var (success, error) = await _billService.ConfirmStripePaymentAsync(
                session.Id, session.PaymentIntentId, amount, billId);

            if (!success)
            {
                _logger.LogError("Failed to confirm Stripe payment for session {SessionId}: {Error}", session.Id, error);
                return StatusCode(StatusCodes.Status500InternalServerError);   // Stripe هتعيد المحاولة
            }

            return Ok();
        }

        private async Task<IActionResult> HandleRefundAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Charge charge || string.IsNullOrEmpty(charge.PaymentIntentId))
                return Ok();

            var (success, error) = await _billService.HandleStripeRefundAsync(
                charge.PaymentIntentId,
                StripeMoney.FromMinorUnits(charge.AmountRefunded, charge.Currency));

            if (!success)
            {
                _logger.LogError("Failed to apply Stripe refund for payment intent {PaymentIntentId}: {Error}", charge.PaymentIntentId, error);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        private async Task<IActionResult> HandleDisputeAsync(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not Dispute dispute) return Ok();

            var (success, error) = await _billService.HandleStripeDisputeAsync(
                dispute.PaymentIntentId,
                StripeMoney.FromMinorUnits(dispute.Amount, dispute.Currency),
                dispute.Reason);

            if (!success)
            {
                _logger.LogError("Failed to record Stripe dispute {DisputeId}: {Error}", dispute.Id, error);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }
    }
}