using ECommerce.Models.Domain.Entities;

namespace ECommerce.Repositories.Interfaces
{
    public interface IProductsRepository
    {
        Task<List<Product>> GetProductsBySellerIdAsync(
            IReadOnlyCollection<Guid> sellerIds,
            IReadOnlyCollection<Guid>? productIds = null
            );

        Task<List<Product>> GetAllProductsAsync();
        Task<Product?> GetListedProductsByIdAsync(Guid productId);
        Task CreateProductAsync(Product product);

        Task SaveChangesAsync();
    }
}
