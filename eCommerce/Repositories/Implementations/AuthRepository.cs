using ECommerce.Data;
using ECommerce.Models.Domain.Exceptions;
using ECommerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ECommerceDbContext dbContext;
        private readonly IConfiguration configuration;

        public AuthRepository(ECommerceDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        public async Task CreateBuyerAsync(Models.Domain.Entities.Buyer buyer)
        {
            try
            {
                await dbContext.AddAsync(buyer);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Buyers_Email" })
            {
                Console.WriteLine(ex);
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
            catch (Exception ex)
                when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "IX_Sellers_Email" })
            {
                Console.WriteLine(ex);
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

        public string GenerateJWT(string userId, string email, string name, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.GivenName, name),
                    new Claim(ClaimTypes.Role, role)
                ]),
                IssuedAt = DateTime.UtcNow,
                Issuer = configuration["JWT:Issuer"],
                Audience = configuration["JWT:Audience"],
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
