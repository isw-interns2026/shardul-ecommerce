using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Interfaces;

namespace ECommerce.Repositories.Implementations
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ECommerceDbContext dbContext;

        public TransactionRepository(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Transaction> CreateTransactionForCartItems(List<CartItem> cartItems)
        {
            decimal amount = 0;

            foreach (var ci in cartItems)
            {
                amount += ci.Product.Price * ci.Count;
            }

            Transaction t = new()
            {
                Amount = amount,
                Status = TransactionStatus.Processing
            };

            await dbContext.AddAsync(t);
            await dbContext.SaveChangesAsync();

            return t;
        }
    }
}
