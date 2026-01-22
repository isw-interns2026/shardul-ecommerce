using ECommerce.Data;
using ECommerce.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Repositories
{
    public class SQLAuthRepository : IAuthRepository
    {
        private readonly ECommerceDbContext dbContext;
        private readonly IConfiguration configuration;

        public SQLAuthRepository(ECommerceDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        public async Task CreateBuyerAsync(Buyer buyer)
        {   
            await dbContext.AddAsync(buyer);
            await dbContext.SaveChangesAsync();
        }

        public async Task CreateSellerAsync(Seller seller)
        {
            await dbContext.AddAsync(seller);
            await dbContext.SaveChangesAsync();
        }

        public async Task<Buyer?> GetBuyerIfValidCredentialsAsync(string email, string password)
        {  
            var buyer = await dbContext.Buyers.FirstOrDefaultAsync(buyer => buyer.Email == email);
            if(buyer != null && BCrypt.Net.BCrypt.EnhancedVerify(password, buyer.PasswordHash))
            {
                return buyer;
            }
            return null;
        }

        public async Task<Seller?> GetSellerIfValidCredentialsAsync(string email, string password)
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
