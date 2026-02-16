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
    [ApiController]
    [Route("seller/orders")]
    [Authorize(Roles = "Seller")]
    public class OrdersController : ControllerBase
    {
        private readonly Guid sellerId;
        private readonly IOrdersRepository ordersRepository;
        private readonly IUnitOfWork unitOfWork;

        public OrdersController(ICurrentUser currentUser, IOrdersRepository ordersRepository, IUnitOfWork unitOfWork)
        {
            sellerId = currentUser.UserId;
            this.ordersRepository = ordersRepository;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSellerOrders()
        {
            List<Order> orders = await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Seller([sellerId]));
            return Ok(orders.Select(o => o.ToSellerOrderDto()).ToList());
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetSellerOrderByOrderID([FromRoute] Guid orderId)
        {
            Order? order = (await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Seller([sellerId]), orderIds: [orderId])).FirstOrDefault();

            if (order is null) return NotFound();

            return Ok(order.ToSellerOrderDto());
        }

        [HttpPatch("{orderId}")]
        public async Task<IActionResult> UpdateOrder([FromRoute] Guid orderId, [FromBody] UpdateOrderDto updateOrderDto)
        {
            Order? order = (await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Seller([sellerId]), orderIds: [orderId])).FirstOrDefault();

            if (order is null) return NotFound();

            order.TransitionTo(updateOrderDto.Status);

            await unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
