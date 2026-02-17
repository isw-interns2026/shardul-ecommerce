using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.IntegrationTests.Helpers;
using ECommerce.Models.DTO.Buyer;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Pipeline;

[Collection("Database")]
public class BuyerProductEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _fixture;

    public BuyerProductEndpointTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetAllProducts_ReturnsOnlyListedProducts()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var listed = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: true);
        var unlisted = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: false);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var products = await client.GetFromJsonAsync<List<BuyerProductResponseDto>>("/buyer/products");

        products.Should().Contain(p => p.Id == listed.Id);
        products.Should().NotContain(p => p.Id == unlisted.Id);
    }

    [Fact]
    public async Task GetProductById_Listed_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: true);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.GetAsync($"/buyer/products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<BuyerProductResponseDto>();
        dto!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task GetProductById_Unlisted_Returns404()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: false);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.GetAsync($"/buyer/products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductById_NonExistent_Returns404()
    {
        await using var db = _fixture.CreateDbContext();
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.GetAsync($"/buyer/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
