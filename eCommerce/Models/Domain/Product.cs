using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.Domain
{
    public class Product : Entity
    {
        public required Guid SellerId { get; set; }
        public Seller Seller { get; set; }

        public required string Sku { get; set; }

        public required string Name { get; set; }

        public required decimal Price { get; set; }

        public required int CountInStock { get; set; }

        public string? Description { get; set; }

        public byte[]? Images { get; set; }

        public required bool IsListed { get; set; }
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

            // Field: Price, CountInStock Constraint: Greater than/equal to zero
            builder.ToTable(t => t.HasCheckConstraint("CK_Product_Price_Positive", "\"Price\" > 0"));
            builder.ToTable(t => t.HasCheckConstraint("CK_Product_Stock_Positive", "\"CountInStock\" >= 0"));
        }
    }
}
