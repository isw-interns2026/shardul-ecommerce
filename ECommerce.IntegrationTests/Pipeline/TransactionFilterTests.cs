using System.Net;
using System.Net.Http.Json;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.IntegrationTests.Helpers;
using ECommerce.Models.DTO.Buyer;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.Pipeline;

[Collection("Database")]
public class TransactionFilterTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly PostgresFixture _fixture;

    public TransactionFilterTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _factory = new CustomWebApplicationFactory(fixture.ConnectionString);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task SuccessfulPost_DataCommitted()
    {
        var email = $"{Guid.NewGuid():N}@test.com";
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/auth/buyer/register", new
        {
            Name = "Commit Test", Email = email, Password = "Test1234!", Address = "123 St"
        });

        // Verify buyer exists in DB
        await using var db = _fixture.CreateDbContext();
        var buyer = await db.Buyers.FirstOrDefaultAsync(b => b.Email == email);
        buyer.Should().NotBeNull();
    }

    [Fact]
    public async Task FailedValidation_DataNotCommitted()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var initialCount = await db.Products.CountAsync(p => p.SellerId == seller.Id);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");

        // Invalid product (price = 0) → validation rejects → rollback
        var response = await client.PostAsJsonAsync("/seller/products", new
        {
            Sku = "ROLLBACK-TEST",
            Name = "Fail",
            Price = 0,         // fails GreaterThan(0) validation
            CountInStock = 1,
            IsListed = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Verify no new product
        await using var verifyDb = _fixture.CreateDbContext();
        var count = await verifyDb.Products.CountAsync(p => p.SellerId == seller.Id);
        count.Should().Be(initialCount);
    }

    [Fact]
    public async Task DomainException_DataNotCommitted()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id, stock: 50, reserved: 30);

        var client = _factory.CreateClient().Authenticate(seller.Id, "Seller");

        // Setting stock below reserved → throws StockBelowReservedException → rollback
        var response = await client.PutAsJsonAsync($"/seller/products/{product.Id}/stock",
            new { CountInStock = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Verify stock unchanged
        await using var verifyDb = _fixture.CreateDbContext();
        var p = await verifyDb.Products.FindAsync(product.Id);
        p!.CountInStock.Should().Be(50);
    }

    [Fact]
    public async Task GetRequest_NoTransactionOverhead_DataReadable()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        TestDatabaseHelper.SeedProduct(db, seller.Id, isListed: true);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var client = _factory.CreateClient().Authenticate(buyer.Id, "Buyer");
        var response = await client.GetAsync("/buyer/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<BuyerProductResponseDto>>();
        products.Should().NotBeEmpty();
    }
}
