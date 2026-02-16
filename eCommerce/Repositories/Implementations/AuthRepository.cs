using ECommerce.Data;
using ECommerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ECommerceDbContext dbContext;

        public AuthRepository(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void CreateBuyer(Models.Domain.Entities.Buyer buyer)
        {
            dbContext.Add(buyer);
        }

        public void CreateSeller(Models.Domain.Entities.Seller seller)
        {
            dbContext.Add(seller);
        }

        public async Task<Models.Domain.Entities.Buyer?> GetBuyerIfValidCredentialsAsync(string email, string password)
        {
            var buyer = await dbContext.Buyers.FirstOrDefaultAsync(buyer => buyer.Email == email);
            if (buyer != null && BCrypt.Net.BCrypt.EnhancedVerify(password, buyer.PasswordHash))
            {
                return buyer;
            }
            return null;
        }

        public async Task<Models.Domain.Entities.Seller?> GetSellerIfValidCredentialsAsync(string email, string password)
        {
            var seller = await dbContext.Sellers.FirstOrDefaultAsync(seller => seller.Email == email);
            if (seller != null && BCrypt.Net.BCrypt.EnhancedVerify(password, seller.PasswordHash))
            {
                return seller;
            }
            return null;
        }
    }
}
