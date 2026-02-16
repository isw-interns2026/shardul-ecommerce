using ECommerce.Models.Domain.Entities;
using ECommerce.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace ECommerce.Services.Implementations
{
    public class StripeService : IStripeService
    {
        private readonly StripeClient stripeClient;
        private readonly IConfiguration configuration;

        public StripeService(IConfiguration configuration)
        {
            this.configuration = configuration;
            stripeClient = new StripeClient(configuration["Stripe:SecretKey"]);
        }

        public async Task<string> CreateCheckoutSessionAsync(Transaction transaction, List<CartItem> cartItems)
        {
            var lineItems = new List<SessionLineItemOptions>();

            foreach (var ci in cartItems)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "inr",
                        UnitAmountDecimal = ci.Product.Price * 100, // Stripe expects amount in paise
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = ci.Product.Name,
                            Description = ci.Product.Sku
                        }
                    },
                    Quantity = ci.Count
                });
            }

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "payment",
                LineItems = lineItems,
                SuccessUrl = configuration["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = configuration["Stripe:CancelUrl"],
                Metadata = new Dictionary<string, string>
                {
                    { "transaction_id", transaction.Id.ToString() }
                }
            };

            var session = await stripeClient.V1.Checkout.Sessions.CreateAsync(sessionOptions);

            // Store the Stripe Session ID on our transaction — caller flushes
            transaction.StripeSessionId = session.Id;

            return session.Url;
        }
    }
}
