using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Implementations
{
    public class StockReservationService : IStockReservationService
    {
        private readonly ECommerceDbContext dbContext;

        public StockReservationService(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task ReserveStockForCartItems(List<CartItem> cartItems)
        {
            foreach (var ci in cartItems)
            {
                var product = await dbContext.Products.FindAsync(ci.ProductId)
                    ?? throw new ProductNotFoundException(ci.ProductId);

                int available = (product.CountInStock ?? 0) - product.ReservedCount;

                if (ci.Count > available)
                    throw new InsufficientStockException(product);

                product.ReservedCount += ci.Count;
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task ConfirmReservation(Guid transactionId)
        {
            var transaction = await dbContext.Set<Transaction>()
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction is null || transaction.Status != TransactionStatus.Processing)
                return; // Idempotent — already handled

            var orders = await dbContext.Orders
                .Include(o => o.Product)
                .Where(o => o.TransactionId == transactionId)
                .ToListAsync();

            foreach (var order in orders)
            {
                var product = order.Product!;
                product.CountInStock -= order.Count;
                product.ReservedCount -= order.Count;
                order.Status = OrderStatus.WaitingForSellerToAccept;
            }

            transaction.Status = TransactionStatus.Success;
            await dbContext.SaveChangesAsync();
        }

        public async Task ReleaseReservation(Guid transactionId)
        {
            var transaction = await dbContext.Set<Transaction>()
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction is null || transaction.Status == TransactionStatus.Success)
                return; // Don't release already-confirmed payments

            var orders = await dbContext.Orders
                .Include(o => o.Product)
                .Where(o => o.TransactionId == transactionId)
                .ToListAsync();

            foreach (var order in orders)
            {
                order.Product!.ReservedCount -= order.Count;
                order.Status = OrderStatus.Cancelled;
            }

            transaction.Status = TransactionStatus.Expired;
            await dbContext.SaveChangesAsync();
        }
    }
}