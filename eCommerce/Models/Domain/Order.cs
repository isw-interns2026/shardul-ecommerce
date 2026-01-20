using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain
{   

    public enum OrderStatus
    {
        Delivered, WaitingForSellerToAccept, InTransit, AwaitingPayment, Cancelled
    }
    public class Order : Entity
    {
        public required Guid ProductId { get; set; }
        public Product Product { get; set; }

        public required Guid BuyerId { get; set; }
        public Buyer Buyer { get; set; }

        public Guid SellerId { get; set; }
        public Seller Seller { get; set; }

        public Guid TransactionId { get; set; }
        public Transaction Transaction { get; set; }

        public required int Count { get; set; }
        public required string Address { get; set; }
        public required OrderStatus Status { get; set; }
    }

    public class OrderConfiguration : EntityConfiguration<Order>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
        {
            //Foreign Key ProductId, BuyerId, SellerId, TransactionId
            builder.HasOne(order => order.Buyer).WithMany().HasForeignKey(order => order.BuyerId).IsRequired();
            builder.HasOne(order => order.Seller).WithMany().HasForeignKey(order => order.SellerId).IsRequired();
            builder.HasOne(order => order.Transaction).WithOne().HasForeignKey<Order>(order => order.TransactionId).IsRequired();
            builder.HasOne(order => order.Product).WithMany().HasForeignKey(order => order.ProductId).IsRequired();

            //Field: Address Constraint: REQUIRED
            builder.Property(order => order.Address).IsRequired();

            //Store Status as a string (preventing default conversion to integer)
            builder.Property(order => order.Status).HasConversion<string>().IsRequired();
        }
    }
}
