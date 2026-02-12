using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace ECommerce.Controllers
{
    [Route("api/stripe/webhook")]
    [ApiController]
    [AllowAnonymous]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStockReservationService reservationService;
        private readonly ECommerceDbContext dbContext;
        private readonly string webhookSecret;
        private readonly ILogger<StripeWebhookController> logger;

        public StripeWebhookController(
            IStockReservationService reservationService,
            ECommerceDbContext dbContext,
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger)
        {
            this.reservationService = reservationService;
            this.dbContext = dbContext;
            this.logger = logger;
            webhookSecret = configuration["Stripe:WebhookSecret"]
                ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            Event? stripeEvent = await VerifyAndParseEvent();
            if (stripeEvent is null)
                return BadRequest("Invalid signature.");

            try
            {
                switch (stripeEvent.Type)
                {
                    case EventTypes.CheckoutSessionCompleted:
                        Transaction confirmedTx = await ResolveTransaction(stripeEvent);
                        logger.LogInformation("Payment confirmed for transaction {TransactionId}", confirmedTx.Id);
                        await reservationService.ConfirmReservation(confirmedTx.Id);
                        break;

                    case EventTypes.CheckoutSessionExpired:
                        Transaction expiredTx = await ResolveTransaction(stripeEvent);
                        logger.LogInformation("Payment expired for transaction {TransactionId}", expiredTx.Id);
                        await reservationService.ReleaseReservation(expiredTx.Id);
                        break;

                    default:
                        logger.LogInformation("Unhandled Stripe event type: {Type}", stripeEvent.Type);
                        break;
                }
            }
            catch (WebhookSessionNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
            }
            catch (WebhookTransactionNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
            }

            // Always return 200 — Stripe retries on non-2xx
            return Ok();
        }

        // ── Helpers ──────────────────────────────────────────────

        private async Task<Event?> VerifyAndParseEvent()
        {
            HttpContext.Request.EnableBuffering();

            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body, leaveOpen: true))
            {
                json = await reader.ReadToEndAsync();
            }

            try
            {
                return EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret,
                    throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                logger.LogWarning(ex, "Stripe webhook signature verification failed");
                return null;
            }
        }

        private async Task<Transaction> ResolveTransaction(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session
                ?? throw new WebhookSessionNotFoundException(stripeEvent.Id);

            return await dbContext.Set<Transaction>()
                .FirstOrDefaultAsync(t => t.StripeSessionId == session.Id)
                ?? throw new WebhookTransactionNotFoundException(session.Id);
        }

        // ── Webhook-specific exceptions (not domain exceptions — always return 200) ──

        private sealed class WebhookSessionNotFoundException(string eventId)
            : Exception($"Could not extract session from Stripe event {eventId}")
        { }

        private sealed class WebhookTransactionNotFoundException(string sessionId)
            : Exception($"No transaction found for Stripe session {sessionId}")
        { }
    }
}
