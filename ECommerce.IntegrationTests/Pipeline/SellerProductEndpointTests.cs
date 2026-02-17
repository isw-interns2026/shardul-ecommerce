using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.IntegrationTests.Helpers;
using ECommerce.Models.DTO.Seller;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Pipeline;

[Collection("Database")]
public class SellerProductEndpointTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _fixture;

    public SellerProductEndpointTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetAllProducts_ReturnsOnlyThisSellersProducts()
    {
        await using var db = _fixture.CreateDbContext();
        var seller1 = TestDatabaseHelper.SeedSeller(db);
        var seller2 = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller1.Id);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller2.Id);

        var client = _factory.CreateClient().Authenticate(seller1.Id, "Seller");
        var products = await client.GetFromJsonAsync<List<SellerProductResponseDto>>("/seller/products");

        products.Should().Contain(p => p.Id == p1.Id);
        products.Should().NotContain(p => p.Id == p2.Id);
    }

    [Fact]
    public async Task GetProductById_OwnProduct_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.GetAsync($"/seller/products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductById_OtherSellersProduct_Returns404()
    {
        await using var db = _fixture.CreateDbContext();
        var seller1 = TestDatabaseHelper.SeedSeller(db);
        var seller2 = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller2.Id);

        var client = _factory.CreateClient().Authenticate(seller1.Id, "Seller");
        var response = await client.GetAsync($"/seller/products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddProduct_ValidData_Returns201()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PostAsJsonAsync("/seller/products", new
        {
            Sku = $"NEW-{Guid.NewGuid():N}"[..15],
            Name = "New Product",
            Price = 29.99m,
            CountInStock = 100,
            IsListed = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddProduct_DuplicateSku_Returns409()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var sku = $"DUP-{Guid.NewGuid():N}"[..15];
        TestDatabaseHelper.SeedProduct(db, seller.Id, sku: sku);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PostAsJsonAsync("/seller/products", new
        {
            Sku = sku,
            Name = "Duplicate",
            Price = 10m,
            CountInStock = 1,
            IsListed = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateProduct_ValidPatch_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PatchAsJsonAsync($"/seller/products/{product.Id}",
            new { Name = "Updated Name" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify update persisted
        var updated = await client.GetFromJsonAsync<SellerProductResponseDto>(
            $"/seller/products/{product.Id}");
        updated!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateProduct_NonExistent_Returns404()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PatchAsJsonAsync($"/seller/products/{Guid.NewGuid()}",
            new { Name = "Nope" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetStock_ValidValue_Returns200()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PutAsJsonAsync($"/seller/products/{product.Id}/stock",
            new { CountInStock = 200 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetStock_BelowReserved_Returns422()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50, reserved: 30);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PutAsJsonAsync($"/seller/products/{product.Id}/stock",
            new { CountInStock = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SetStock_NonExistentProduct_Returns404()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");
        var response = await client.PutAsJsonAsync($"/seller/products/{Guid.NewGuid()}/stock",
            new { CountInStock = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
