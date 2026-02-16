using ECommerce.Models.Domain.Entities;
using ECommerce.Models.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Reflection;

namespace ECommerce.Data
{
    public class ECommerceDbContext : DbContext, IUnitOfWork
    {
        public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Entity>();
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
            {
                throw pg.ConstraintName switch
                {
                    "IX_Buyers_Email"          => new DuplicateEmailException(),
                    "IX_Sellers_Email"         => new DuplicateEmailException(),
                    "IX_Products_Sku_SellerId" => new DuplicateSkuException(),
                    _                          => ex
                };
            }
        }

        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
    }
}
