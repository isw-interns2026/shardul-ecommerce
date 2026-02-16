using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Models.Domain.Entities
{

    public enum OrderStatus
    {
        Delivered, InTransit, AwaitingPayment, Cancelled
    }
    public class Order : Entity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid BuyerId { get; set; }
        public Buyer? Buyer { get; set; }

        public Guid SellerId { get; set; }
        public Seller? Seller { get; set; }

        public Guid TransactionId { get; set; }
        public Transaction? Transaction { get; set; }

        public int Count { get; set; }
        public decimal Total { get; set; }
        public string? Address { get; set; }
        public OrderStatus Status { get; private set; }

        private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
        {
            { OrderStatus.InTransit, [OrderStatus.Delivered] }
        };

        private Order() { }

        public void TransitionTo(OrderStatus newStatus)
        {
            if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
                throw new Exceptions.InvalidOrderStatusTransitionException(Status, newStatus);

            Status = newStatus;
        }

        /// <summary>System-level: payment confirmed, move to fulfillment.</summary>
        internal void MarkInTransit() => Status = OrderStatus.InTransit;

        /// <summary>System-level: payment failed or reservation expired.</summary>
        internal void MarkCancelled() => Status = OrderStatus.Cancelled;

        public static Order Create(Buyer b, CartItem ci, Transaction t)
        {
            decimal total = ci.Product.Price * ci.Count;

            var order = new Order
            {
                Address = b.Address,
                BuyerId = b.Id,
                Count = ci.Count,
                ProductId = ci.ProductId,
                SellerId = ci.Product.SellerId,
                Total = total,
                Status = OrderStatus.AwaitingPayment,
                Transaction = t
            };

            return order;
        }
    }

    public class OrderConfiguration : EntityConfiguration<Order>
    {
        protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
        {
            //Foreign Key ProductId, BuyerId, SellerId, TransactionId
            builder.HasOne(order => order.Buyer).WithMany().HasForeignKey(order => order.BuyerId).IsRequired();
            builder.HasOne(order => order.Seller).WithMany().HasForeignKey(order => order.SellerId).IsRequired();
            builder.HasOne(order => order.Transaction).WithMany().HasForeignKey(order => order.TransactionId).IsRequired();
            builder.HasOne(order => order.Product).WithMany().HasForeignKey(order => order.ProductId).IsRequired();

            //Field:Address Constraint:REQUIRED
            builder.Property(order => order.Address).IsRequired();

            //Store Status as a string (preventing default conversion to integer)
            builder.Property(order => order.Status).HasConversion<string>().IsRequired();

            //Field:Total Constraint:Greater than or equal to 0
            builder.ToTable(t => t.HasCheckConstraint("CK_Order_Total_Positive", "\"Total\" >= 0"));

            //Field:Count Constraint:Greater than 0
            builder.ToTable(t => t.HasCheckConstraint("CK_Ordered_Product_Count_Positive", "\"Count\" > 0"));
        }
    }
}
