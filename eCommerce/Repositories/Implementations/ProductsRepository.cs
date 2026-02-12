using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Products_Sku_SellerId" })
            {
                throw new DuplicateSkuException();
            }
        }

        public async Task<List<Product>> GetAllListedProductsAsync()
        {
            return await dbContext.Products.Where(p => p.IsListed).ToListAsync();
        }

        public async Task<Product?> GetListedProductsByIdAsync(Guid productId)
        {
            return await dbContext.Products.Where(product => product.Id == productId && product.IsListed == true).FirstOrDefaultAsync();
        }
    }
}
