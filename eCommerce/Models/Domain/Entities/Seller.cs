using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain.Entities
{
    public class Seller : Entity
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string BankAccountNumber { get; set; } = null!;
        public ICollection<Product> Products { get; set; } = new List<Product>();

        private Seller() { }

        public static Seller Create(string name, string email, string passwordHash, string bankAccountNumber)
        {
            return new Seller
            {
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                BankAccountNumber = bankAccountNumber
            };
        }
    }

    public class SellerConfiguration : EntityConfiguration<Seller>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Seller> builder)
        {
            // Field: Email, Constraint: UNIQUE and Required
            builder.HasIndex(seller => seller.Email).IsUnique();
            builder.Property(seller => seller.Email).IsRequired();

            // Fields: Name, BankAccountNumber, PasswordHash Constraint: Required
            builder.Property(seller => seller.Name).IsRequired();
            builder.Property(seller => seller.BankAccountNumber).IsRequired();
            builder.Property(seller => seller.PasswordHash).IsRequired();
        }
    }
}
