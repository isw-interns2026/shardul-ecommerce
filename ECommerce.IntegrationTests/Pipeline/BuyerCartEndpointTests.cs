using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.IntegrationTests.Helpers;
using ECommerce.Models.DTO.Buyer;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Pipeline;

[Collection("Database")]
public class BuyerCartEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _fixture;

    public BuyerCartEndpointTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task AddToCart_ValidProduct_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.PostAsync($"/buyer/cart/{product.Id}?count=3", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddToCart_UpdateExistingCount_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 2);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.PostAsync($"/buyer/cart/{product.Id}?count=5", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify count updated
        var items = await client.GetFromJsonAsync<List<BuyerCartItemResponseDto>>("/buyer/cart");
        items.Should().ContainSingle(i => i.Id == product.Id && i.CountInCart == 5);
    }

    [Fact]
    public async Task AddToCart_NonPositiveCount_Returns400()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.PostAsync($"/buyer/cart/{Guid.NewGuid()}?count=0", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCart_ReturnsItemsWithProductData()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 3);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var items = await client.GetFromJsonAsync<List<BuyerCartItemResponseDto>>("/buyer/cart");

        items.Should().ContainSingle();
        items![0].Name.Should().Be(product.Name);
        items[0].CountInCart.Should().Be(3);
    }

    [Fact]
    public async Task DeleteFromCart_Returns204()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 2);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.DeleteAsync($"/buyer/cart/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ClearCart_Returns204_AllItemsRemoved()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p1.Id, count: 1);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p2.Id, count: 2);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.DeleteAsync("/buyer/cart");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var items = await client.GetFromJsonAsync<List<BuyerCartItemResponseDto>>("/buyer/cart");
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task PlaceOrders_WithItems_Returns200WithCheckoutUrl()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 2);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.PostAsync("/buyer/cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body.Should().ContainKey("checkoutUrl");
        body!["checkoutUrl"].Should().Contain("fake-checkout.stripe.com");
    }

    [Fact]
    public async Task PlaceOrders_EmptyCart_Returns400()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.PostAsync("/buyer/cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PlaceOrders_InsufficientStock_Returns422()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 2);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 10);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.PostAsync("/buyer/cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
