using AutoMapper;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Buyer;
using ECommerce.Models.DTO.Seller;
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
        private readonly IMapper mapper;
        private readonly Guid buyerId;

        public BuyerOrdersController(IOrdersRepository ordersRepository, IMapper mapper, ICurrentUser currentUser)
        {
            this.ordersRepository = ordersRepository;
            this.mapper = mapper;
            buyerId = currentUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBuyerOrders()
        {
            List<Order> orders = await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Buyer([buyerId]));

            var buyerOrderResponseDtos = new List<BuyerOrderResponseDto>();

            foreach (Order o in orders)
                buyerOrderResponseDtos.Add(mapper.Map<BuyerOrderResponseDto>(o));

            return Ok(buyerOrderResponseDtos);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetSellerOrderByOrderID([FromRoute] Guid orderId)
        {
            Order? order = (await ordersRepository.GetOrdersAsync(new MandatoryUserIdArgument.Buyer([buyerId]), orderIds: [orderId])).FirstOrDefault();

            if (order is null) return NotFound();

            var sellerOrderResponseDto = mapper.Map<SellerOrderResponseDto>(order);
            return Ok(sellerOrderResponseDto);
        }
    }
}
