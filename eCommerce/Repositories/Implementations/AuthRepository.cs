using ECommerce.Data;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ECommerceDbContext dbContext;

        public AuthRepository(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task CreateBuyerAsync(Models.Domain.Entities.Buyer buyer)
        {
            try
            {
                await dbContext.AddAsync(buyer);
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Buyers_Email" })
            {
                throw new DuplicateEmailException();
            }
        }

        public async Task CreateSellerAsync(Models.Domain.Entities.Seller seller)
        {
            try
            {
                await dbContext.AddAsync(seller);
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Sellers_Email" })
            {
                throw new DuplicateEmailException();
            }
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
