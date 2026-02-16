using ECommerce.Models.Domain.Entities;

namespace ECommerce.Models.DTO.Buyer
{
    public class BuyerOrderResponseDto
    {
        public required Guid OrderId { get; set; }
        public required decimal OrderValue { get; set; }
        public required int ProductCount { get; set; }
        public required Guid ProductId { get; set; }
        public required string ProductName { get; set; }
        public required string ProductSku { get; set; }
        public required string DeliveryAddress { get; set; }
        public required OrderStatus OrderStatus { get; set; }
    }
}
