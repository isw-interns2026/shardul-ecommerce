using ECommerce.Data;
using ECommerce.Mappings;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Buyer;
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
        private readonly IAuthRepository authRepository;
        private readonly ICartRepository cartRepository;
        private readonly IOrdersRepository ordersRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IStockReservationService stockReservationService;
        private readonly IStripeService stripeService;
        private readonly IUnitOfWork unitOfWork;

        public BuyerCartController(
            IAuthRepository authRepository,
            ICartRepository cartRepository,
            IOrdersRepository ordersRepository,
            ITransactionRepository transactionRepository,
            IStockReservationService stockReservationService,
            IStripeService stripeService,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            this.authRepository = authRepository;
            this.cartRepository = cartRepository;
            this.ordersRepository = ordersRepository;
            this.transactionRepository = transactionRepository;
            this.stockReservationService = stockReservationService;
            this.stripeService = stripeService;
            this.unitOfWork = unitOfWork;
            buyerId = currentUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            List<CartItem> cartItems = await cartRepository.GetBuyerCartItemsAsync(buyerId);

            var buyerCartItemResponseDtos = new List<BuyerCartItemResponseDto>();

            foreach (var cartItem in cartItems)
            {
                var dto = cartItem.Product.ToBuyerCartItemDto();
                dto.CountInCart = cartItem.Count;
                buyerCartItemResponseDtos.Add(dto);
            }

            return Ok(buyerCartItemResponseDtos);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddOrUpdateCart([FromRoute] Guid productId, [FromQuery] int count)
        {
            if (count <= 0)
                return BadRequest("Count is non-positive.");

            await cartRepository.AddOrUpdateCartAsync(buyerId: buyerId, productId: productId, count: count);
            await unitOfWork.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProductFromCart([FromRoute] Guid productId)
        {
            await cartRepository.DeleteProductFromCartAsync(buyerId: buyerId, productId: productId);
            await unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await cartRepository.ClearCartAsync(buyerId);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrders()
        {
            List<CartItem> cartItems = await cartRepository.GetBuyerCartItemsAsync(buyerId);

            if (cartItems.Count == 0)
                return BadRequest("Cart is empty.");

            Models.Domain.Entities.Buyer b = await authRepository.GetBuyerByIdAsync(buyerId);

            await stockReservationService.ReserveStockForCartItems(cartItems);

            Transaction t = transactionRepository.CreateTransactionForCartItems(cartItems);
            ordersRepository.CreateOrdersForTransaction(cartItems: cartItems, buyer: b, transaction: t);

            await unitOfWork.SaveChangesAsync();

            string checkoutUrl = await stripeService.CreateCheckoutSessionAsync(t, cartItems);

            await unitOfWork.SaveChangesAsync();

            await cartRepository.ClearCartAsync(buyerId);

            return Ok(new { checkoutUrl });
        }
    }
}
