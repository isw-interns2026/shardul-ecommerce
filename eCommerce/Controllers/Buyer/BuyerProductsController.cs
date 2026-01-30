using AutoMapper;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Buyer;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Buyer
{
    [Route("buyer/products")]
    [ApiController]
    [Authorize(Roles = "Buyer")]
    public class BuyerProductsController : ControllerBase
    {
        private readonly IProductsRepository productsRepository;
        private readonly IMapper mapper;
        private readonly Guid buyerId;

        public BuyerProductsController(IProductsRepository productsRepository, IMapper mapper, ICurrentUser currentUser)
        {
            this.productsRepository = productsRepository;
            this.mapper = mapper;
            buyerId = currentUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            List<Product> products = await productsRepository.GetAllProductsAsync();

            var buyerProductResponseDtos = new List<BuyerProductResponseDto>();

            foreach (Product p in products)
                buyerProductResponseDtos.Add(mapper.Map<BuyerProductResponseDto>(p));

            return Ok(buyerProductResponseDtos);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductByProductID([FromRoute] Guid productId)
        {
            Product? product = await productsRepository.GetListedProductsByIdAsync(productId);

            if (product is null) return NotFound();

            var buyerProductResponseDto = mapper.Map<BuyerProductResponseDto>(product);
            return Ok(buyerProductResponseDto);
        }
    }
}
