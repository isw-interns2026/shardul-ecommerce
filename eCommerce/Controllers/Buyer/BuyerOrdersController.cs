using ECommerce.Mappings;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Buyer
{
    [Route("buyer/orders")]
    [ApiController]
    [Authorize(Roles = "Buyer")]
    public class BuyerOrdersController : ControllerBase
    {
        private readonly IOrdersRepository ordersRepository;
        private readonly Guid buyerId;

        public BuyerOrdersController(IOrdersRepository ordersRepository, ICurrentUser currentUser)
        {
            this.ordersRepository = ordersRepository;
            buyerId = currentUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBuyerOrders()
        {
            List<Order> orders = await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Buyer([buyerId]));
            return Ok(orders.Select(o => o.ToBuyerOrderDto()).ToList());
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
        {
            Order? order = (await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Buyer([buyerId]), orderIds: [orderId])).FirstOrDefault();

            if (order is null) return NotFound();

            return Ok(order.ToBuyerOrderDto());
        }
    }
}
