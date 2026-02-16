using ECommerce.Models.Domain.Entities;

namespace ECommerce.Repositories.Interfaces
{
    public interface IProductsRepository
    {
        Task<List<Product>> GetProductsBySellerIdAsync(
            IReadOnlyCollection<Guid> sellerIds,
            IReadOnlyCollection<Guid>? productIds = null
            );

        Task<List<Product>> GetAllListedProductsAsync();
        Task<Product?> GetListedProductsByIdAsync(Guid productId);
        void CreateProduct(Product product);
    }
}
