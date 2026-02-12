using AutoMapper;
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
        private readonly ICartRepository cartRepository;
        private readonly IOrdersRepository ordersRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IStockReservationService stockReservationService;
        private readonly IStripeService stripeService;
        private readonly IMapper mapper;

        public BuyerCartController(
            ICartRepository cartRepository,
            IOrdersRepository ordersRepository,
            ITransactionRepository transactionRepository,
            IStockReservationService stockReservationService,
            IStripeService stripeService,
            IMapper mapper,
            ICurrentUser currentUser)
        {
            this.cartRepository = cartRepository;
            this.ordersRepository = ordersRepository;
            this.transactionRepository = transactionRepository;
            this.stockReservationService = stockReservationService;
            this.stripeService = stripeService;
            this.mapper = mapper;
            buyerId = currentUser.UserId;
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            List<CartItem> cartItems = await cartRepository.GetBuyerCartItemsAsync(buyerId);

            var buyerCartItemResponseDtos = new List<BuyerCartItemResponseDto>();

            foreach (var cartItem in cartItems)
            {
                var cartItemResponseDto = mapper.Map<BuyerCartItemResponseDto>(cartItem.Product);
                cartItemResponseDto.CountInCart = cartItem.Count;
                buyerCartItemResponseDtos.Add(cartItemResponseDto);
            }

            return Ok(buyerCartItemResponseDtos);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddOrUpdateCart([FromRoute] Guid productId, [FromQuery] int count)
        {
            if (count <= 0)
                return BadRequest("Count is non-positive.");

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

            Models.Domain.Entities.Buyer b = await cartRepository.GetBuyerById(buyerId);

            // 1. Reserve stock — throws InsufficientStockException if unavailable
            await stockReservationService.ReserveStockForCartItems(cartItems);

            // 2. Create transaction (status = Processing)
            Transaction t = await transactionRepository.CreateTransactionForCartItems(cartItems);

            // 3. Create orders (status = AwaitingPayment)
            await ordersRepository.CreateOrdersForTransaction(cartItems: cartItems, buyer: b, transaction: t);

            // 4. Create Stripe Checkout Session (if this fails, cart is still intact)
            string checkoutUrl = await stripeService.CreateCheckoutSessionAsync(t, cartItems);

            // 5. Clear cart only after Stripe session is created successfully
            await cartRepository.ClearCartAsync(buyerId);

            return Ok(new { checkoutUrl });
        }
    }
}