using AutoMapper;
using ECommerce.Models.Domain.Entities;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers.Buyer
{

    [Route("buyer/cart")]
    [ApiController]
    [Authorize(Roles = "Buyer")]
    public class BuyerCartController : ControllerBase
    {
        private readonly Guid buyerId;
        private readonly ICartRepository cartRepository;
        private readonly IOrdersRepository ordersRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IMapper mapper;

        public BuyerCartController(
            ICartRepository cartRepository,
            IOrdersRepository ordersRepository,
            ITransactionRepository transactionRepository,
            IMapper mapper,
            ICurrentUser currentUser)
        {
            this.cartRepository = cartRepository;
            this.ordersRepository = ordersRepository;
            this.transactionRepository = transactionRepository;
            this.mapper = mapper;
            buyerId = currentUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            List<CartItem> cartItems = await cartRepository.GetBuyerCartItemsAsync(buyerId);
            return Ok(cartItems);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddOrUpdateCart([FromRoute] Guid productId, [FromQuery] int count)
        {
            await cartRepository.AddOrUpdateCart(buyerId: buyerId, productId: productId, count: count);
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProductFromCart([FromRoute] Guid productId)
        {
            await cartRepository.DeleteProductFromCart(buyerId: buyerId, productId: productId);
            return NoContent();
        }

        [HttpDelete]
        public Task<IActionResult> ClearCart()
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrders()
        {
            List<CartItem> cartItems = await cartRepository.GetBuyerCartItemsAsync(buyerId);
            Models.Domain.Entities.Buyer b = await cartRepository.GetBuyerById(buyerId);

            Transaction t = await transactionRepository.CreateTransactionForCartItems(cartItems);

            await cartRepository.SubtractCartItemsFromProductStock(cartItems);

            await ordersRepository.CreateOrdersForTransaction(cartItems: cartItems, buyer: b, transaction: t);

            //TODO: Call transaction payment handler with transaction id.

            return Ok();
        }
    }
}
