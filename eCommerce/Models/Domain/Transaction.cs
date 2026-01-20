using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain
{
    public enum TransactionStatus
    {
        Success, Failed, Processing
    }
    public class Transaction : Entity
    {
        public required Guid SellerId { get; set; }
        public Seller Seller { get; set; }

        public required decimal Amount { get; set; }

        public required TransactionStatus Status { get; set; }
    }

    public class TransactionConfiguration : EntityConfiguration<Transaction>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Transaction> builder)
        {
            //Foreign Key SellerId
            builder.HasOne(transaction => transaction.Seller).WithMany().HasForeignKey(transaction => transaction.SellerId).IsRequired();

            //Field: Amount Constraint: REQUIRED
            builder.Property(transaction => transaction.Amount).IsRequired();

            //Store Status as a string (preventing default conversion to integer)
            builder.Property(transaction => transaction.Status).HasConversion<string>().IsRequired();
        }
    }

}
