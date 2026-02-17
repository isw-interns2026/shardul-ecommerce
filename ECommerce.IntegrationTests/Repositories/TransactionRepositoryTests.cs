using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Implementations;
using FluentAssertions;

namespace ECommerce.IntegrationTests.Repositories;

[Collection("Database")]
public class TransactionRepositoryTests
{
    private readonly PostgresFixture _fixture;

    public TransactionRepositoryTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CreateTransactionForCartItems_ComputesCorrectAmount()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var p1 = TestDatabaseHelper.SeedProduct(db, seller.Id, price: 25.50m);
        var p2 = TestDatabaseHelper.SeedProduct(db, seller.Id, price: 10.00m);
        var buyer = TestDatabaseHelper.SeedBuyer(db);

        var ci1 = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p1.Id, count: 2);
        var ci2 = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, p2.Id, count: 3);
        db.Entry(ci1).Reference(c => c.Product).Load();
        db.Entry(ci2).Reference(c => c.Product).Load();

        var repo = new TransactionRepository(db);
        var tx = repo.CreateTransactionForCartItems([ci1, ci2]);
        await db.SaveChangesAsync();

        // (25.50 * 2) + (10.00 * 3) = 51.00 + 30.00 = 81.00
        tx.Amount.Should().Be(81.00m);
    }

    [Fact]
    public async Task CreateTransactionForCartItems_SetsStatusProcessing()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var ci = TestDatabaseHelper.SeedCartItem(db, buyer.Cart.Id, product.Id, count: 1);
        db.Entry(ci).Reference(c => c.Product).Load();

        var repo = new TransactionRepository(db);
        var tx = repo.CreateTransactionForCartItems([ci]);
        await db.SaveChangesAsync();

        tx.Status.Should().Be(TransactionStatus.Processing);
    }

    [Fact]
    public async Task GetByStripeSessionId_Found_ReturnsTransaction()
    {
        await using var db = _fixture.CreateDbContext();
        var sessionId = $"cs_test_{Guid.NewGuid():N}";
        var tx = new Transaction
        {
            Amount = 100,
            Status = TransactionStatus.Processing,
            StripeSessionId = sessionId
        };
        db.Add(tx);
        await db.SaveChangesAsync();

        var repo = new TransactionRepository(db);
        var result = await repo.GetByStripeSessionIdAsync(sessionId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(tx.Id);
    }

    [Fact]
    public async Task GetByStripeSessionId_NotFound_ReturnsNull()
    {
        await using var db = _fixture.CreateDbContext();
        var repo = new TransactionRepository(db);
        var result = await repo.GetByStripeSessionIdAsync("cs_nonexistent_session");

        result.Should().BeNull();
    }
}
