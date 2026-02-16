using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Implementations
{
    public class StockReservationService : IStockReservationService
    {
        private const int MaxRetries = 3;
        private readonly ECommerceDbContext dbContext;

        public StockReservationService(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task ReserveStockForCartItems(List<CartItem> cartItems) =>
            ExecuteWithRetry(async () =>
            {
                foreach (var ci in cartItems)
                {
                    var product = await dbContext.Products.FindAsync(ci.ProductId)
                        ?? throw new ProductNotFoundException(ci.ProductId);

                    if (ci.Count > product.AvailableStock)
                        throw new InsufficientStockException(product);

                    product.ReservedCount += ci.Count;
                }

                await dbContext.SaveChangesAsync();
            });

        public Task ConfirmReservation(Guid transactionId) =>
            ExecuteWithRetry(async () =>
            {
                var transaction = await FindTransaction(transactionId);

                if (transaction is null || transaction.Status != TransactionStatus.Processing)
                    return;

                var orders = await GetOrdersWithProducts(transactionId);

                foreach (var order in orders)
                {
                    order.Product!.CountInStock -= order.Count;
                    order.Product!.ReservedCount -= order.Count;
                    order.MarkInTransit();
                }

                transaction.Status = TransactionStatus.Success;
                await dbContext.SaveChangesAsync();
            });

        public Task ReleaseReservation(Guid transactionId) =>
            ExecuteWithRetry(async () =>
            {
                var transaction = await FindTransaction(transactionId);

                if (transaction is null || transaction.Status != TransactionStatus.Processing)
                    return;

                var orders = await GetOrdersWithProducts(transactionId);

                foreach (var order in orders)
                {
                    order.Product!.ReservedCount -= order.Count;
                    order.MarkCancelled();
                }

                transaction.Status = TransactionStatus.Expired;
                await dbContext.SaveChangesAsync();
            });

        // ── Helpers ──────────────────────────────────────────────

        private Task<Transaction?> FindTransaction(Guid transactionId) =>
            dbContext.Set<Transaction>()
                .FirstOrDefaultAsync(t => t.Id == transactionId);

        private Task<List<Order>> GetOrdersWithProducts(Guid transactionId) =>
            dbContext.Orders
                .Include(o => o.Product)
                .Where(o => o.TransactionId == transactionId)
                .ToListAsync();

        private async Task ExecuteWithRetry(Func<Task> operation)
        {
            for (int attempt = 0; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (DbUpdateConcurrencyException ex) when (attempt < MaxRetries)
                {
                    await RefreshConflictingProducts(ex);
                }
            }
        }

        /// <summary>
        /// Refreshes only the Product entities that failed the concurrency check,
        /// in a single round trip. Preserves all other tracked entities.
        /// </summary>
        private async Task RefreshConflictingProducts(DbUpdateConcurrencyException ex)
        {
            var failedProductIds = ex.Entries
                .Where(e => e.Entity is Product)
                .Select(e => ((Product)e.Entity).Id)
                .ToList();

            if (failedProductIds.Count == 0)
                throw ex;

            var freshProducts = await dbContext.Products
                .Where(p => failedProductIds.Contains(p.Id))
                .AsNoTracking()
                .ToDictionaryAsync(p => p.Id);

            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is Product p && freshProducts.TryGetValue(p.Id, out var fresh))
                {
                    entry.CurrentValues.SetValues(fresh);
                    entry.OriginalValues.SetValues(fresh);
                }
            }
        }
    }
}
