using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
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

        public async Task AddOrUpdateCart(Guid buyerId, Guid productId, int count)
        {
            Guid cartId = await dbContext.Carts
            .Where(c => c.BuyerId == buyerId)
            .Select(c => c.Id).FirstAsync();

            var cartItem = await dbContext.CartItems.Where(ci => ci.CartId == cartId && ci.ProductId == productId).FirstOrDefaultAsync();

            if (cartItem != null)
            {
                cartItem.Count = count;
            }
            else
            {
                cartItem = new CartItem { CartId = cartId, ProductId = productId, Count = count };
                await dbContext.AddAsync(cartItem);
            }

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.Message);
                throw new InvalidCartItemException();
            }
        }

        public async Task DeleteProductFromCart(Guid buyerId, Guid productId)
        {
            Guid cartId = await dbContext.Carts
                        .Where(c => c.BuyerId == buyerId)
                        .Select(c => c.Id).FirstAsync();

            var cartItem = await dbContext.CartItems.Where(ci => ci.CartId == cartId && ci.ProductId == productId).FirstOrDefaultAsync();

            if (cartItem != null)
            {
                dbContext.CartItems.Remove(cartItem);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<Buyer> GetBuyerById(Guid buyerId)
        {
            Buyer b = await dbContext.Buyers.Where(b => b.Id == buyerId).FirstAsync();
            return b;
        }

        public async Task SubtractCartItemsFromProductStock(List<CartItem> cartItems)
        {
            foreach (var ci in cartItems)
            {
                try
                {
                    ci.Product.CountInStock -= ci.Count;
                    await dbContext.SaveChangesAsync();
                }
                catch
                {
                    throw new InsufficientStockException(ci.Product);
                }
            }
        }
    }
}
