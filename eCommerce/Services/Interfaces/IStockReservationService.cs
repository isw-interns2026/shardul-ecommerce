using ECommerce.Models.Domain.Entities;

namespace ECommerce.Services.Interfaces
{
    public interface IStockReservationService
    {
        /// <summary>
        /// Reserves stock for all cart items. Increments Product.ReservedCount.
        /// Throws InsufficientStockException if not enough available stock.
        /// </summary>
        Task ReserveStockForCartItems(List<CartItem> cartItems);

        /// <summary>
        /// Payment succeeded. Converts reservation into actual stock subtraction:
        /// CountInStock -= count, ReservedCount -= count. Order status → WaitingForSellerToAccept.
        /// </summary>
        Task ConfirmReservation(Guid transactionId);

        /// <summary>
        /// Payment failed or expired. Releases reservation:
        /// ReservedCount -= count. Order status → Cancelled.
        /// </summary>
        Task ReleaseReservation(Guid transactionId);
    }
}