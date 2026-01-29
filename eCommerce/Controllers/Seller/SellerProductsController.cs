using AutoMapper;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Seller;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Controllers.Seller
{
    [Route("seller/products")]
    [ApiController]
    [Authorize(Roles = "Seller")]
    public class ProductsController : ControllerBase
    {
        private readonly Guid sellerId;
        private readonly IProductsRepository productsRepository;
        private readonly IMapper mapper;

        public ProductsController(ICurrentUser currentUser, IProductsRepository productsRepository, IMapper mapper)
        {
            sellerId = currentUser.UserId;
            this.productsRepository = productsRepository;
            this.mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllSellerProducts()
        {
            List<Product> products = await productsRepository.GetProductsBySellerIdAsync(sellerIds: [sellerId]);

            var sellerProductResponseDtos = new List<SellerProductResponseDto>();

            foreach (Product p in products)
                sellerProductResponseDtos.Add(mapper.Map<SellerProductResponseDto>(p));

            return Ok(sellerProductResponseDtos);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetSellerProductByProductID([FromRoute] Guid productId)
        {
            Product? product = (await productsRepository.GetProductsBySellerIdAsync(productIds: [productId], sellerIds: [sellerId])).FirstOrDefault();

            if (product is null) return NotFound();

            var sellerProductResponseDto = mapper.Map<SellerProductResponseDto>(product);
            return Ok(sellerProductResponseDto);
        }

        [HttpPatch("{productId}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid productId, [FromBody] UpdateProductDto updateProductDto)
        {
            Product? product = (await productsRepository.GetProductsBySellerIdAsync(productIds: [productId], sellerIds: [sellerId])).FirstOrDefault();

            if (product is null) return NotFound();

            mapper.Map(updateProductDto, product);

            await productsRepository.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] AddProductDto addProductDto)
        {
            Product p = mapper.Map<Product>(addProductDto);
            p.SellerId = sellerId;

            await productsRepository.CreateProductAsync(p);

            return Created();
        }

    }
}
