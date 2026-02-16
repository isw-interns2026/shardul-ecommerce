using ECommerce.Mappings;
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
        private readonly Guid buyerId;

        public BuyerProductsController(IProductsRepository productsRepository, ICurrentUser currentUser)
        {
            this.productsRepository = productsRepository;
            buyerId = currentUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            List<Product> products = await productsRepository.GetAllListedProductsAsync();
            return Ok(products.Select(p => p.ToBuyerProductDto()).ToList());
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductByProductID([FromRoute] Guid productId)
        {
            Product? product = await productsRepository.GetListedProductsByIdAsync(productId);

            if (product is null) return NotFound();

            return Ok(product.ToBuyerProductDto());
        }
    }
}
