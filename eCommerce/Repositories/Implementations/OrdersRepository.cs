using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ECommerce.Repositories.Implementations
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly ECommerceDbContext dbContext;

        public OrdersRepository(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Order>> GetOrdersAsync(
            MandatoryUserIdArgument endUserIds,
            IReadOnlyCollection<Guid>? orderIds = null,
            IReadOnlyCollection<Guid>? productIds = null
            )
        {
            IQueryable<Order> query = dbContext.Orders;

            query = endUserIds switch
            {
                MandatoryUserIdArgument.Seller s =>
                    query.Where(o => s.SellerIds.Contains(o.SellerId)),

                MandatoryUserIdArgument.Buyer b =>
                    query.Where(o => b.BuyerIds.Contains(o.BuyerId)),

                _ => throw new UnreachableException()
            };

            if (orderIds is { Count: > 0 })
                query = query.Where(o => orderIds.Contains(o.Id));

            if (productIds is { Count: > 0 })
                query = query.Where(o => productIds.Contains(o.ProductId));

            return await query.Include(o => o.Product).ToListAsync();
        }

        public void CreateOrdersForTransaction(List<CartItem> cartItems, Buyer buyer, Transaction transaction)
        {
            foreach (var ci in cartItems)
            {
                Order o = Order.Create(buyer, ci, transaction);
                dbContext.Add(o);
            }
        }
    }
}
