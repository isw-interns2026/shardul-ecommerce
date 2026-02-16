using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain.Entities
{
    public class CartItem : Entity
    {
        public required Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public required Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Count { get; set; } = 0;
    }

    public class CartItemConfiguration : EntityConfiguration<CartItem>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<CartItem> builder)
        {   
            //Foreign Key CartId, ProductId
            builder.HasOne(ci => ci.Cart).WithMany(c => c.Items).HasForeignKey(ci => ci.CartId).IsRequired();
            builder.HasOne(ci => ci.Product).WithMany().HasForeignKey(ci => ci.ProductId).IsRequired();

            //Composite key (CartId, ProductId)
            builder.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();

            //Count must be positive
            builder.ToTable(t => t.HasCheckConstraint("CK_CartItem_Count_Positive", "\"Count\" > 0"));
        }
    }   
}
