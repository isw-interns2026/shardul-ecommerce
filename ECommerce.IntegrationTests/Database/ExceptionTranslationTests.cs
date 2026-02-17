using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.Database;

[Collection("Database")]
public class ExceptionTranslationTests
{
    private readonly PostgresFixture _fixture;

    public ExceptionTranslationTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task DuplicateBuyerEmail_ThrowsDuplicateEmailException()
    {
        var email = $"{Guid.NewGuid():N}"[..8];

        await using var db1 = _fixture.CreateDbContext();
        TestDatabaseHelper.SeedBuyer(db1, email);

        await using var db2 = _fixture.CreateDbContext();
        var duplicate = Buyer.Create("Dup", $"{email}@test.com", "hash", "addr");
        db2.Add(duplicate);

        await db2.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task DuplicateSellerEmail_ThrowsDuplicateEmailException()
    {
        var email = $"{Guid.NewGuid():N}"[..8];

        await using var db1 = _fixture.CreateDbContext();
        TestDatabaseHelper.SeedSeller(db1, email);

        await using var db2 = _fixture.CreateDbContext();
        var duplicate = Seller.Create("Dup", $"{email}@test.com", "hash", "bank");
        db2.Add(duplicate);

        await db2.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task DuplicateSkuSameSeller_ThrowsDuplicateSkuException()
    {
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var sku = $"DUP-{Guid.NewGuid():N}"[..15];

        TestDatabaseHelper.SeedProduct(db, seller.Id, sku: sku);

        var duplicate = new Product
        {
            SellerId = seller.Id, Sku = sku,
            Name = "Dup", Price = 10, CountInStock = 1, IsListed = true
        };
        db.Add(duplicate);

        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DuplicateSkuException>();
    }

    [Fact]
    public async Task SameSkuDifferentSeller_Succeeds()
    {
        await using var db = _fixture.CreateDbContext();
        var seller1 = TestDatabaseHelper.SeedSeller(db);
        var seller2 = TestDatabaseHelper.SeedSeller(db);
        var sku = $"SHARED-{Guid.NewGuid():N}"[..15];

        TestDatabaseHelper.SeedProduct(db, seller1.Id, sku: sku);
        TestDatabaseHelper.SeedProduct(db, seller2.Id, sku: sku);

        // No exception — composite key allows same SKU for different sellers
    }

    [Fact]
    public async Task UnknownUniqueViolation_RethrowsDbUpdateException()
    {
        // The StripeSessionId unique index is not in the translation switch.
        // Violating it should throw the original DbUpdateException, not a domain exception.
        await using var db = _fixture.CreateDbContext();
        var seller = TestDatabaseHelper.SeedSeller(db);
        var buyer = TestDatabaseHelper.SeedBuyer(db);
        var product = TestDatabaseHelper.SeedProduct(db, seller.Id);

        var tx1 = new Transaction { Amount = 100, Status = TransactionStatus.Processing, StripeSessionId = "cs_dup_test" };
        var tx2 = new Transaction { Amount = 200, Status = TransactionStatus.Processing, StripeSessionId = "cs_dup_test" };

        db.Add(tx1);
        await db.SaveChangesAsync();

        db.Add(tx2);

        // Should throw DbUpdateException (not translated to a domain exception)
        await db.Invoking(d => d.SaveChangesAsync())
            .Should().ThrowAsync<DbUpdateException>();
    }
}
