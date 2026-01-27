using ECommerce.Models.Domain.Entities;

namespace ECommerce.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetBuyerCartItemsAsync(Guid buyerId);
        Task AddOrUpdateCart(Guid buyerId, Guid productId, int count);
        Task DeleteProductFromCart(Guid buyerId, Guid productId);
        Task<Buyer> GetBuyerById(Guid buyerId);
        Task SubtractCartItemsFromProductStock(List<CartItem> cartItems);
    }
}
