using ECommerce.Data;
using ECommerce.Models.DTO.Auth.Request;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    [AllowAnonymous]
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly ITokenService tokenService;
        private readonly IUnitOfWork unitOfWork;

        public AuthController(IAuthRepository authRepository, ITokenService tokenService, IUnitOfWork unitOfWork)
        {
            this.authRepository = authRepository;
            this.tokenService = tokenService;
            this.unitOfWork = unitOfWork;
        }

        [HttpPost("buyer/register")]
        public async Task<IActionResult> RegisterBuyer([FromBody] BuyerRegisterRequestDto Dto)
        {
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(Dto.Password);

            var newBuyer = Models.Domain.Entities.Buyer.Create(
                name: Dto.Name,
                email: Dto.Email,
                passwordHash: hashedPassword,
                address: Dto.Address
                );

            authRepository.CreateBuyer(newBuyer);
            await unitOfWork.SaveChangesAsync();

            return Ok("Buyer created.");
        }

        [HttpPost("seller/register")]
        public async Task<IActionResult> RegisterSeller([FromBody] SellerRegisterRequestDto Dto)
        {
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(Dto.Password);

            var newSeller = Models.Domain.Entities.Seller.Create(
                name: Dto.Name,
                email: Dto.Email,
                passwordHash: hashedPassword,
                bankAccountNumber: Dto.BankAccountNumber
            );

            authRepository.CreateSeller(newSeller);
            await unitOfWork.SaveChangesAsync();

            return Ok("Seller created.");
        }

        [HttpPost("buyer/login")]
        public async Task<IActionResult> LoginBuyer([FromBody] LoginRequestDto Dto)
        {
            var buyer = await authRepository.GetBuyerIfValidCredentialsAsync(email: Dto.Email, password: Dto.Password);

            if (buyer is null)
                return Unauthorized();

            var accessToken = tokenService.GenerateJWT(userId: buyer.Id.ToString(), email: buyer.Email, name: buyer.Name, role: "Buyer");

            return Ok(new { accessToken });
        }

        [HttpPost("seller/login")]
        public async Task<IActionResult> LoginSeller([FromBody] LoginRequestDto Dto)
        {
            var seller = await authRepository.GetSellerIfValidCredentialsAsync(email: Dto.Email, password: Dto.Password);

            if (seller is null)
                return Unauthorized();

            var accessToken = tokenService.GenerateJWT(userId: seller.Id.ToString(), email: seller.Email, name: seller.Name, role: "Seller");

            return Ok(new { accessToken });
        }
    }
}
