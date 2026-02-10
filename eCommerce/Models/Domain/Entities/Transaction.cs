using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain.Entities
{
    public enum TransactionStatus
    {
        Success, Failed, Processing, Rollback, Expired
    }

    public class Transaction : Entity
    {
        public decimal? Amount { get; set; }
        public TransactionStatus? Status { get; set; }

        /// <summary>
        /// Extracts the creation timestamp from the UUIDv7 primary key.
        /// This is when stock was reserved, used by the cleanup job.
        /// </summary>
        public DateTime CreatedAt
        {
            get
            {
                var hex = Id.ToString("N").Substring(0, 12);
                var ms = Convert.ToInt64(hex, 16);
                return DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;
            }
        }
    }

    public class TransactionConfiguration : EntityConfiguration<Transaction>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Transaction> builder)
        {
            builder.Property(t => t.Amount).IsRequired();
            builder.Property(t => t.Status).HasConversion<string>().IsRequired();

            // CreatedAt is computed from Id — not a database column
            builder.Ignore(t => t.CreatedAt);
        }
    }
}