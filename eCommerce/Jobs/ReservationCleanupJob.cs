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

        // v10: constructor injection works directly — no ServiceScope needed
        public ReservationCleanupJob(
            ECommerceDbContext dbContext,
            IStockReservationService reservationService,
            ILogger<ReservationCleanupJob> logger)
        {
            this.dbContext = dbContext;
            this.reservationService = reservationService;
            this.logger = logger;
        }

        // v10: 6-part cron (with seconds). "0 */5 * * * *" = at second 0, every 5th minute.
        [TickerFunction(
            functionName: "ExpireStaleReservations",
            cronExpression: "0 */5 * * * *")]
        public async Task ExecuteAsync(
            TickerFunctionContext context,
            CancellationToken cancellationToken)
        {
            var cutoff = DateTime.UtcNow - ReservationTimeout;

            var processingTransactions = await dbContext.Set<Transaction>()
                .Where(t => t.Status == TransactionStatus.Processing)
                .ToListAsync(cancellationToken);

            var staleTransactionIds = processingTransactions
                .Where(t => t.CreatedAt < cutoff)
                .Select(t => t.Id)
                .ToList();

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