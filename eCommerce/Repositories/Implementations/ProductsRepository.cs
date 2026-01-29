using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Repositories.Implementations
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly ECommerceDbContext dbContext;

        public ProductsRepository(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Product>> GetProductsBySellerIdAsync(
            IReadOnlyCollection<Guid>? sellerIds = null,
            IReadOnlyCollection<Guid>? productIds = null
            )
        {
            IQueryable<Product> query = dbContext.Products;

            if (sellerIds is { Count: > 0 })
                query = query.Where(p => sellerIds.Contains(p.SellerId));

            if (productIds is { Count: > 0 })
                query = query.Where(p => productIds.Contains(p.Id));

            return await query.ToListAsync();
        }

        public async Task CreateProductAsync(Product p)
        {
            try
            {
                await dbContext.AddAsync(p);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw new DuplicateSkuException();
            }
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await dbContext.Products.ToListAsync();
        }

        public async Task<Product?> GetProductsByIdAsync(Guid productId)
        {
            return await dbContext.Products.Where(product => product.Id == productId).FirstOrDefaultAsync();
        }
    }
}
