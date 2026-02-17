using ECommerce.Data;
using ECommerce.IntegrationTests.Fixtures;
using ECommerce.Models.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IntegrationTests.CleanupJob;

[Collection("Database")]
public class ReservationCleanupQueryTests
{
    private readonly PostgresFixture _fixture;

    public ReservationCleanupQueryTests(PostgresFixture fixture) => _fixture = fixture;

    /// <summary>
    /// Runs the same raw SQL the cleanup job uses, parameterized with a cutoff time.
    /// </summary>
    private static async Task<List<Guid>> GetStaleTransactionIds(ECommerceDbContext db, DateTime cutoff)
    {
        return await db.Set<Transaction>()
            .FromSqlRaw(
                """
                SELECT * FROM "Transactions"
                WHERE "Status" = 'Processing'
                  AND to_timestamp(
                        ('x' || lpad(replace(left("Id"::text, 13), '-', ''), 12, '0'))::bit(48)::bigint
                        / 1000.0
                      ) < {0}
                """,
                cutoff)
            .Select(t => t.Id)
            .ToListAsync();
    }

    [Fact]
    public async Task StaleProcessingTransaction_FoundByQuery()
    {
        await using var db = _fixture.CreateDbContext();

        // Transaction created now with UUIDv7 → timestamp = now
        var tx = new Transaction { Amount = 100, Status = TransactionStatus.Processing };
        db.Add(tx);
        await db.SaveChangesAsync();

        // Cutoff in the future → this transaction is "stale" relative to it
        var futureUtoff = DateTime.UtcNow.AddMinutes(30);
        var result = await GetStaleTransactionIds(db, futureUtoff);

        result.Should().Contain(tx.Id);
    }

    [Fact]
    public async Task RecentProcessingTransaction_NotFoundByQuery()
    {
        await using var db = _fixture.CreateDbContext();

        var tx = new Transaction { Amount = 100, Status = TransactionStatus.Processing };
        db.Add(tx);
        await db.SaveChangesAsync();

        // Cutoff in the past → this transaction is "recent" relative to it
        var pastCutoff = DateTime.UtcNow.AddMinutes(-30);
        var result = await GetStaleTransactionIds(db, pastCutoff);

        result.Should().NotContain(tx.Id);
    }

    [Fact]
    public async Task StaleButExpiredTransaction_NotFoundByQuery()
    {
        await using var db = _fixture.CreateDbContext();

        var tx = new Transaction { Amount = 100, Status = TransactionStatus.Expired };
        db.Add(tx);
        await db.SaveChangesAsync();

        var futureCutoff = DateTime.UtcNow.AddMinutes(30);
        var result = await GetStaleTransactionIds(db, futureCutoff);

        result.Should().NotContain(tx.Id);
    }

    [Fact]
    public async Task StaleButSuccessTransaction_NotFoundByQuery()
    {
        await using var db = _fixture.CreateDbContext();

        var tx = new Transaction { Amount = 100, Status = TransactionStatus.Success };
        db.Add(tx);
        await db.SaveChangesAsync();

        var futureCutoff = DateTime.UtcNow.AddMinutes(30);
        var result = await GetStaleTransactionIds(db, futureCutoff);

        result.Should().NotContain(tx.Id);
    }

    [Fact]
    public async Task NoStaleTransactions_ReturnsEmpty()
    {
        await using var db = _fixture.CreateDbContext();

        // All existing Processing transactions are recent
        var pastCutoff = DateTime.UtcNow.AddHours(-24);
        var result = await GetStaleTransactionIds(db, pastCutoff);

        // We can't guarantee zero because other tests seed data,
        // but none of them should have 24-hour-old UUIDv7 timestamps
        result.Should().BeEmpty();
    }
}
