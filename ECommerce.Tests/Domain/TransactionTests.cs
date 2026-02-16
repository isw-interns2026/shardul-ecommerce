using ECommerce.Models.Domain.Entities;
using FluentAssertions;

namespace ECommerce.Tests.Domain;

public class TransactionTests
{
    // #24
    [Fact]
    public void CreatedAt_ExtractsCorrectTimestampFromUuidV7()
    {
        // Guid.CreateVersion7 encodes Unix-ms in the first 48 bits.
        // Capture the time before and after, verify CreatedAt falls within.
        var before = DateTime.UtcNow.AddSeconds(-1);
        var transaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            Amount = 100m,
            Status = TransactionStatus.Processing
        };
        var after = DateTime.UtcNow.AddSeconds(1);

        transaction.CreatedAt.Should().BeOnOrAfter(before);
        transaction.CreatedAt.Should().BeOnOrBefore(after);
    }
}
