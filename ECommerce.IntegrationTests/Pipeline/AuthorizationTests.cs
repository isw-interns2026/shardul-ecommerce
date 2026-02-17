using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.IntegrationTests.Helpers;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Pipeline;

[Collection("Database")]
public class AuthorizationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthorizationTests(PostgresFixture fixture)
    {
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task BuyerEndpoint_NoToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/buyer/products");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SellerEndpoint_NoToken_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/seller/products");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SellerEndpoint_BuyerToken_Returns403()
    {
        var client = _factory.CreateClient().Authenticate(Guid.NewGuid(), "Buyer");
        var response = await client.GetAsync("/seller/products");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task BuyerEndpoint_SellerToken_Returns403()
    {
        var client = _factory.CreateClient().Authenticate(Guid.NewGuid(), "Seller");
        var response = await client.GetAsync("/buyer/cart");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AuthEndpoint_NoToken_Accessible()
    {
        var client = _factory.CreateClient();
        // Login with bad creds should return 401, not redirect — proving endpoint is reachable
        var response = await client.PostAsJsonAsync("/auth/buyer/login",
            new { Email = "no@test.com", Password = "Test1234!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
