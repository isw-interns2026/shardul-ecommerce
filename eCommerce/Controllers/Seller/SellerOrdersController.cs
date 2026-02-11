
using AutoMapper;
using ECommerce.Data;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Seller;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Seller
{
    [ApiController]
    [Route("seller/orders")]
    [Authorize(Roles = "Seller")]
    public class OrdersController : ControllerBase
    {
        private readonly Guid sellerId;
        private readonly IOrdersRepository ordersRepository;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public OrdersController(ICurrentUser currentUser, IOrdersRepository ordersRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            sellerId = currentUser.UserId;
            this.ordersRepository = ordersRepository;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllSellerOrders()
        {
            List<Order> orders = await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Seller([sellerId]));

            var sellerOrderResponseDtos = new List<SellerOrderResponseDto>();

            foreach (Order o in orders)
                sellerOrderResponseDtos.Add(mapper.Map<SellerOrderResponseDto>(o));

            return Ok(sellerOrderResponseDtos);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetSellerOrderByOrderID([FromRoute] Guid orderId)
        {
            Order? order = (await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Seller([sellerId]), orderIds: [orderId])).FirstOrDefault();

            if (order is null) return NotFound();

            var sellerOrderResponseDto = mapper.Map<SellerOrderResponseDto>(order);
            return Ok(sellerOrderResponseDto);
        }

        [HttpPatch("{orderId}")]
        public async Task<IActionResult> UpdateOrder([FromRoute] Guid orderId, [FromBody] UpdateOrderDto updateOrderDto)
        {
            Order? order = (await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Seller([sellerId]), orderIds: [orderId])).FirstOrDefault();

            if (order is null) return NotFound();

            mapper.Map(updateOrderDto, order);

            await unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
