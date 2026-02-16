using ECommerce.Models.Domain.Entities;

namespace ECommerce.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetBuyerCartItemsAsync(Guid buyerId);
        Task AddOrUpdateCartAsync(Guid buyerId, Guid productId, int count);
        Task DeleteProductFromCartAsync(Guid buyerId, Guid productId);
        Task ClearCartAsync(Guid buyerId);
    }
}