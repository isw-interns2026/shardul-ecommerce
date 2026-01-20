using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain
{
    public class Seller : Entity
    {
        public required string Name { get; set; } = null!;
        public required string Email { get; set; } = null!;
        public required string PasswordHash { get; set; } = null!;
        public required string Ban { get; set; } = null!;
        public ICollection<Product> Products { get; set;} = new List<Product>();
    }

    public class SellerConfiguration : EntityConfiguration<Seller>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Seller> builder)
        {
            // Field: Email, Constraint: UNIQUE and Required
            builder.HasIndex(seller => seller.Email).IsUnique();
            builder.Property(seller => seller.Email).IsRequired();
                                  
            // Fields: Username, Ban, Password_Hash Constraint: Required
            builder.Property(seller => seller.Name).IsRequired();
            builder.Property(seller => seller.Ban).IsRequired();
            builder.Property(seller => seller.PasswordHash).IsRequired();

        }
    }
}
