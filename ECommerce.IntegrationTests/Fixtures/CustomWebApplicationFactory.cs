using ECommerce.Data;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ECommerce.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public IStripeService StripeMock { get; }

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;

        StripeMock = Substitute.For<IStripeService>();
        StripeMock
            .CreateCheckoutSessionAsync(Arg.Any<Models.Domain.Entities.Transaction>(), Arg.Any<List<Models.Domain.Entities.CartItem>>())
            .Returns(callInfo =>
            {
                // Set StripeSessionId on the transaction (mimics real StripeService)
                var tx = callInfo.ArgAt<Models.Domain.Entities.Transaction>(0);
                tx.StripeSessionId = $"cs_test_{Guid.NewGuid():N}";
                return "https://fake-checkout.stripe.com/session";
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace ECommerceDbContext with Testcontainers PostgreSQL
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ECommerceDbContext>)
                         || d.ServiceType == typeof(ECommerceDbContext)
                         || d.ServiceType == typeof(IUnitOfWork))
                .ToList();

            foreach (var d in dbDescriptors)
                services.Remove(d);

            services.AddDbContext<ECommerceDbContext>(options =>
                options.UseNpgsql(_connectionString));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ECommerceDbContext>());

            // Replace Stripe with mock
            var stripeDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IStripeService));
            if (stripeDescriptor != null) services.Remove(stripeDescriptor);
            services.AddSingleton(StripeMock);
        });

        // Provide all required configuration for fail-fast validation
        builder.UseSetting("ConnectionStrings:PgConnString", _connectionString);
        builder.UseSetting("JWT:SecretKey", "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!!");
        builder.UseSetting("JWT:Issuer", "test-issuer");
        builder.UseSetting("JWT:Audience", "test-audience");
        builder.UseSetting("Stripe:SecretKey", "sk_test_fake");
        builder.UseSetting("Stripe:WebhookSecret", "whsec_test_fake");
        builder.UseSetting("Stripe:SuccessUrl", "https://localhost/success");
        builder.UseSetting("Stripe:CancelUrl", "https://localhost/cancel");
    }
}
