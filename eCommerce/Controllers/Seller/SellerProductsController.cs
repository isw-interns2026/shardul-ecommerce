using ECommerce.Data;
using ECommerce.Mappings;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Seller;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Seller
{
    [Route("seller/products")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class ProductsController : ControllerBase
    {
        private readonly Guid sellerId;
        private readonly IProductsRepository productsRepository;
        private readonly IUnitOfWork unitOfWork;

        public ProductsController(ICurrentUser currentUser, IProductsRepository productsRepository, IUnitOfWork unitOfWork)
        {
            sellerId = currentUser.UserId;
            this.productsRepository = productsRepository;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSellerProducts()
        {
            List<Product> products = await productsRepository.GetProductsBySellerIdAsync(sellerIds: [sellerId]);
            return Ok(products.Select(p => p.ToSellerProductDto()).ToList());
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetSellerProductByProductID([FromRoute] Guid productId)
        {
            Product? product = (await productsRepository.GetProductsBySellerIdAsync(productIds: [productId], sellerIds: [sellerId])).FirstOrDefault();

            if (product is null) return NotFound();

            return Ok(product.ToSellerProductDto());
        }

        [HttpPatch("{productId}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid productId, [FromBody] UpdateProductDto updateProductDto)
        {
            Product? product = (await productsRepository.GetProductsBySellerIdAsync(productIds: [productId], sellerIds: [sellerId])).FirstOrDefault();

            if (product is null) return NotFound();

            ProductUpdateMapper.ApplyUpdate(updateProductDto, product);

            await unitOfWork.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] AddProductDto addProductDto)
        {
            Product p = addProductDto.ToProduct();
            p.SellerId = sellerId;

            productsRepository.CreateProduct(p);
            await unitOfWork.SaveChangesAsync();

            return Created();
        }

        [HttpPut("{productId}/stock")]
        public async Task<IActionResult> SetStock([FromRoute] Guid productId, [FromBody] SetStockDto setStockDto)
        {
            Product? product = (await productsRepository.GetProductsBySellerIdAsync(productIds: [productId], sellerIds: [sellerId])).FirstOrDefault();

            if (product is null) return NotFound();

            if (setStockDto.CountInStock < product.ReservedCount)
                throw new Models.Domain.Exceptions.StockBelowReservedException(product, setStockDto.CountInStock);

            product.CountInStock = setStockDto.CountInStock;

            await unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
