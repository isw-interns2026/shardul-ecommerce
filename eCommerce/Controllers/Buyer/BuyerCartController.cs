using AutoMapper;
using ECommerce.Data;
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
        private readonly IUnitOfWork unitOfWork;

        public BuyerCartController(
            ICartRepository cartRepository,
            IOrdersRepository ordersRepository,
            ITransactionRepository transactionRepository,
            IStockReservationService stockReservationService,
            IStripeService stripeService,
            IMapper mapper,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            this.cartRepository = cartRepository;
            this.ordersRepository = ordersRepository;
            this.transactionRepository = transactionRepository;
            this.stockReservationService = stockReservationService;
            this.stripeService = stripeService;
            this.mapper = mapper;
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

            Models.Domain.Entities.Buyer b = await cartRepository.GetBuyerByIdAsync(buyerId);

            // 1. Reserve stock — flushes internally (concurrency tokens require it)
            await stockReservationService.ReserveStockForCartItems(cartItems);

            // 2. Stage transaction + orders on the change tracker (no flush)
            Transaction t = transactionRepository.CreateTransactionForCartItems(cartItems);
            ordersRepository.CreateOrdersForTransaction(cartItems: cartItems, buyer: b, transaction: t);

            // 3. Flush transaction + orders together
            await unitOfWork.SaveChangesAsync();

            // 4. Create Stripe Checkout Session — sets StripeSessionId on the tracked transaction
            string checkoutUrl = await stripeService.CreateCheckoutSessionAsync(t, cartItems);

            // 5. Flush the StripeSessionId update
            await unitOfWork.SaveChangesAsync();

            // 6. Clear cart
            await cartRepository.ClearCartAsync(buyerId);

            return Ok(new { checkoutUrl });
        }
    }
}
