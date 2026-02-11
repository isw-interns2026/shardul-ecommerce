using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain.Entities
{
    public class Product : Entity
    {
        public Guid SellerId { get; set; }
        public Seller Seller { get; set; }
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public int? CountInStock { get; set; }
        public int ReservedCount { get; set; } = 0;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsListed { get; set; }

        /// <summary>
        /// Available stock = total stock minus currently reserved units.
        /// This is what buyers should see.
        /// </summary>
        public int AvailableStock => (CountInStock ?? 0) - ReservedCount;
    }

    public class ProductConfiguration : EntityConfiguration<Product>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Product> builder)
        {
            //SellerId foreign key, required
            builder.HasOne(product => product.Seller).WithMany(seller => seller.Products).HasForeignKey(product => product.SellerId);
            builder.Property(product => product.SellerId).IsRequired();
            builder.Navigation(product => product.Seller).IsRequired();
            //Sku required
            builder.Property(product => product.Sku).IsRequired();

            // Sku + SellerId composite key
            builder.HasIndex(product => new { product.Sku, product.SellerId }).IsUnique();

            // Field: Name Constraint: Required
            builder.Property(product => product.Name).IsRequired();

            // Ignore the computed property — EF should not try to map it
            builder.Ignore(p => p.AvailableStock);

            //Add concurrency tokens to stock-related fields to prevent lost update problem
            builder.Property(p => p.CountInStock).IsConcurrencyToken();
            builder.Property(p => p.ReservedCount).IsConcurrencyToken();

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Product_Price_Positive", "\"Price\" > 0");
                t.HasCheckConstraint("CK_Product_Stock_Positive", "\"CountInStock\" >= 0");
                t.HasCheckConstraint("CK_Product_Reserved_Non_Negative", "\"ReservedCount\" >= 0");
                t.HasCheckConstraint("CK_Product_Reserved_Within_Stock", "\"ReservedCount\" <= \"CountInStock\"");
            });
        }
    }
}
