using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace ECommerce.Services.Implementations
{
    public class StripeService : IStripeService
    {
        private readonly StripeClient stripeClient;
        private readonly ECommerceDbContext dbContext;
        private readonly IConfiguration configuration;

        public StripeService(ECommerceDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
            stripeClient = new StripeClient(configuration["Stripe:SecretKey"]);
        }

        public async Task<string> CreateCheckoutSessionAsync(Transaction transaction, List<Order> orders)
        {
            var lineItems = new List<SessionLineItemOptions>();

            foreach (var order in orders)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "inr",
                        UnitAmountDecimal = order.Product!.Price * 100, // Stripe expects amount in paise
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = order.Product.Name,
                            Description = order.Product.Sku
                        }
                    },
                    Quantity = order.Count
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

            // Store the Stripe Session ID on our transaction
            transaction.StripeSessionId = session.Id;
            await dbContext.SaveChangesAsync();

            return session.Url;
        }
    }
}
