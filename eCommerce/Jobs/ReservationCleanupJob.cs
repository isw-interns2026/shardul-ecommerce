using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities.Base;

namespace ECommerce.Jobs
{
    public class ReservationCleanupJob
    {
        private static readonly TimeSpan ReservationTimeout = TimeSpan.FromMinutes(15);

        private readonly ECommerceDbContext dbContext;
        private readonly IStockReservationService reservationService;
        private readonly ILogger<ReservationCleanupJob> logger;

        public ReservationCleanupJob(
            ECommerceDbContext dbContext,
            IStockReservationService reservationService,
            ILogger<ReservationCleanupJob> logger)
        {
            this.dbContext = dbContext;
            this.reservationService = reservationService;
            this.logger = logger;
        }

        [TickerFunction(
            functionName: "ExpireStaleReservations",
            cronExpression: "0 */5 * * * *")]
        public async Task ExecuteAsync(
            TickerFunctionContext context,
            CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow - ReservationTimeout;

            // Extract the Unix-ms timestamp from the first 48 bits of the UUIDv7 primary key
            // and filter server-side instead of loading all Processing transactions into memory.
            var staleTransactionIds = await dbContext.Set<Transaction>()
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
                .ToListAsync(cancellationToken);

            if (staleTransactionIds.Count == 0)
                return;

            logger.LogInformation(
                "Found {Count} stale reservations to expire (older than {Cutoff})",
                staleTransactionIds.Count, cutoff);

            foreach (var transactionId in staleTransactionIds)
            {
                try
                {
                    await reservationService.ReleaseReservation(transactionId);
                    logger.LogInformation("Expired reservation for transaction {Id}", transactionId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to expire reservation for transaction {Id}", transactionId);
                }
            }
        }
    }
}
