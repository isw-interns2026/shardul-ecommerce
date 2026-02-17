using ECommerce.Data;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Models.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.Database;

[Collection("Database")]
public class CheckConstraintTests
{
    private readonly PostgresFixture _fixture;

    public CheckConstraintTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Product_PriceZero_Rejected()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var product = new Product
        {
            SellerId = seller.Id, Sku = $"T-{Guid.NewGuid():N}"[..15],
            Name = "Test", Price = 0, CountInStock = 1, IsListed = true
        };
        db.Add(product);

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Product_NegativeStock_Rejected()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var product = new Product
        {
            SellerId = seller.Id, Sku = $"T-{Guid.NewGuid():N}"[..15],
            Name = "Test", Price = 10, CountInStock = -1, IsListed = true
        };
        db.Add(product);

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Product_NegativeReservedCount_Rejected()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var product = new Product
        {
            SellerId = seller.Id, Sku = $"T-{Guid.NewGuid():N}"[..15],
            Name = "Test", Price = 10, CountInStock = 10, ReservedCount = -1, IsListed = true
        };
        db.Add(product);

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Product_ReservedCountExceedsStock_Rejected()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);

        var product = new Product
        {
            SellerId = seller.Id, Sku = $"T-{Guid.NewGuid():N}"[..15],
            Name = "Test", Price = 10, CountInStock = 5, ReservedCount = 10, IsListed = true
        };
        db.Add(product);

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Order_CountZero_Rejected()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db);

        // Directly modify count to 0 to bypass domain logic
        var order = await db.Orders.FirstAsync(o => o.TransactionId == scenario.Transaction.Id);
        order.Count = 0;

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Order_NegativeTotal_Rejected()
    {
        await using var db = _fixture.CreateDbContext();
        var scenario = TestDatabaseHelper.SeedFullCheckoutScenario(db);

        var order = await db.Orders.FirstAsync(o => o.TransactionId == scenario.Transaction.Id);
        order.Total = -1;

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task CartItem_CountZero_Rejected()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var cartItem = new CartItem
        {
            CartId = buyer.Cart.Id,
            ProductId = product.Id,
            Count = 0
        };
        db.Add(cartItem);

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }
}
