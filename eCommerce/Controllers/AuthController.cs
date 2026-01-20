using ECommerce.Data;
using ECommerce.Models.Domain;
using ECommerce.Models.DTO.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ECommerceDbContext dbContext;

        public AuthController(ECommerceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("buyer/register")]
        public async Task<IActionResult> RegisterBuyer([FromBody] BuyerRegisterRequestDto Dto)
        {
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(Dto.Password);

            var newBuyer = Models.Domain.Buyer.Create(
                name: Dto.Name, 
                email: Dto.Email, 
                passwordHash: hashedPassword, 
                address: Dto.Address
                );

            dbContext.Add(newBuyer);
            try
            {
                dbContext.SaveChanges();
            }
            catch
            {
                return BadRequest("Email already exists.");
            }
            return Ok("Buyer created.");
        }

        [HttpPost("seller/register")]
        public async Task<IActionResult> RegisterSeller([FromBody] SellerRegisterRequestDto Dto)
        {
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(Dto.Password);

            var newSeller = new Models.Domain.Seller
            {
                Name = Dto.Name,
                PasswordHash = hashedPassword,
                Email = Dto.Email,
                Ban = Dto.Ban
            };

            dbContext.Add(newSeller);
            try
            {
                dbContext.SaveChanges();
            }
            catch
            {
                return BadRequest("Email already exists.");
            }
            return Ok("Seller created.");
        }
    }
}
