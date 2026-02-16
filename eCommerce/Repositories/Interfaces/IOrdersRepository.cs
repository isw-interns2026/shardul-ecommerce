using ECommerce.Models.Domain.Entities;

namespace ECommerce.Repositories.Interfaces
{
    public abstract record MandatoryUserIdArgument
    {
        private MandatoryUserIdArgument() { }

        public sealed record Seller(IReadOnlyCollection<Guid> SellerIds) : MandatoryUserIdArgument;

        public sealed record Buyer(IReadOnlyCollection<Guid> BuyerIds) : MandatoryUserIdArgument;
    }

    public interface IOrdersRepository
    {
        Task<List<Order>> GetOrdersAsync(
            MandatoryUserIdArgument endUserIds,
            IReadOnlyCollection<Guid>? orderIds = null,
            IReadOnlyCollection<Guid>? productIds = null
            );

        void CreateOrdersForTransaction(List<CartItem> cartItems, Buyer buyer, Transaction transaction);

    }
}
