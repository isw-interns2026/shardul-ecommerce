using ECommerce.Models.Domain.Entities;

namespace ECommerce.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Transaction CreateTransactionForCartItems(List<CartItem> cartItems);
    }
}
