using ECommerce.Models.DTO.Auth;
using ECommerce.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            this.authRepository = authRepository;
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

            try
            {
                await authRepository.CreateBuyerAsync(newBuyer);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict("Email already exists.");
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

            try
            {
                await authRepository.CreateSellerAsync(newSeller);
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict("Email already exists.");
            }

            return Ok("Seller created.");
        }


        [HttpPost("buyer/login")]
        public async Task<IActionResult> LoginBuyer([FromBody] LoginRequestDto Dto)
        {
            var buyer = await authRepository.GetBuyerIfValidCredentialsAsync(email: Dto.Email, password: Dto.Password);

            if (buyer is null)
                return Unauthorized();

            var accessToken = authRepository.GenerateJWT(userId: buyer.Id.ToString(), email: buyer.Email, name: buyer.Name, role: "Buyer");

            return Ok(new { accessToken });
        }

        [HttpPost("seller/login")]
        public async Task<IActionResult> LoginSeller([FromBody] LoginRequestDto Dto)
        {
            var seller = await authRepository.GetSellerIfValidCredentialsAsync(email: Dto.Email, password: Dto.Password);

            if (seller is null)
                return Unauthorized();

            var accessToken = authRepository.GenerateJWT(userId: seller.Id.ToString(), email: seller.Email, name: seller.Name, role: "Seller");

            return Ok(new { accessToken });
        }
    }
}
