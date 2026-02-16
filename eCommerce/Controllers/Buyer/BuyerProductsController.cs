using ECommerce.Mappings;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Interfaces;
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

        public BuyerProductsController(IProductsRepository productsRepository)
        {
            this.productsRepository = productsRepository;
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
