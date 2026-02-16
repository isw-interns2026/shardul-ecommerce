using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Repositories.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly ECommerceDbContext dbContext;

        public CartRepository(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<CartItem>> GetBuyerCartItemsAsync(Guid buyerId)
        {
            Guid cartId = await dbContext.Carts
                        .Where(c => c.BuyerId == buyerId)
                        .Select(c => c.Id).FirstAsync();

            var cartItems = await dbContext.CartItems
                            .Where(ci => ci.CartId == cartId)
                            .Include(ci => ci.Product)
                            .ToListAsync();

            return cartItems;
        }

        public async Task AddOrUpdateCartAsync(Guid buyerId, Guid productId, int count)
        {
            Guid cartId = await dbContext.Carts
                .Where(c => c.BuyerId == buyerId)
                .Select(c => c.Id).FirstAsync();

            var cartItem = await dbContext.CartItems
                .Where(ci => ci.CartId == cartId && ci.ProductId == productId)
                .FirstOrDefaultAsync();

            if (cartItem != null)
            {
                cartItem.Count = count;
            }
            else
            {
                cartItem = new CartItem { CartId = cartId, ProductId = productId, Count = count };
                dbContext.Add(cartItem);
            }
        }

        public async Task DeleteProductFromCartAsync(Guid buyerId, Guid productId)
        {
            Guid cartId = await dbContext.Carts
                        .Where(c => c.BuyerId == buyerId)
                        .Select(c => c.Id).FirstAsync();

            var cartItem = await dbContext.CartItems
                .Where(ci => ci.CartId == cartId && ci.ProductId == productId)
                .FirstOrDefaultAsync();

            if (cartItem != null)
            {
                dbContext.CartItems.Remove(cartItem);
            }
        }

        public async Task ClearCartAsync(Guid buyerId)
        {
            Guid cartId = await dbContext.Carts
                .Where(c => c.BuyerId == buyerId)
                .Select(c => c.Id)
                .FirstAsync();

            await dbContext.CartItems
                .Where(ci => ci.CartId == cartId)
                .ExecuteDeleteAsync();
        }
    }
}
