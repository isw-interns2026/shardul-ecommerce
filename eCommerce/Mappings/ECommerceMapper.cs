using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Buyer;
using ECommerce.Models.DTO.Seller;
using Riok.Mapperly.Abstractions;

namespace ECommerce.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class ECommerceMapper
    {
        // ── Product ──────────────────────────────────────────────

        public static partial BuyerProductResponseDto ToBuyerProductDto(this Product product);

        [MapperIgnoreTarget(nameof(BuyerCartItemResponseDto.CountInCart))]
        public static partial BuyerCartItemResponseDto ToBuyerCartItemDto(this Product product);

        public static partial SellerProductResponseDto ToSellerProductDto(this Product product);

        [MapperIgnoreTarget(nameof(Product.Id))]
        [MapperIgnoreTarget(nameof(Product.SellerId))]
        [MapperIgnoreTarget(nameof(Product.Seller))]
        [MapperIgnoreTarget(nameof(Product.ReservedCount))]
        public static partial Product ToProduct(this AddProductDto dto);

        // ── Order ────────────────────────────────────────────────

        public static BuyerOrderResponseDto ToBuyerOrderDto(this Order order)
        {
            ArgumentNullException.ThrowIfNull(order.Product,
                "Order.Product must be included before mapping. Use .Include(o => o.Product).");
            return MapToBuyerOrderDto(order);
        }

        public static SellerOrderResponseDto ToSellerOrderDto(this Order order)
        {
            ArgumentNullException.ThrowIfNull(order.Product,
                "Order.Product must be included before mapping. Use .Include(o => o.Product).");
            return MapToSellerOrderDto(order);
        }

        [MapProperty(nameof(Order.Id), nameof(BuyerOrderResponseDto.OrderId))]
        [MapProperty(nameof(Order.Total), nameof(BuyerOrderResponseDto.OrderValue))]
        [MapProperty(nameof(Order.Count), nameof(BuyerOrderResponseDto.ProductCount))]
        [MapProperty(nameof(Order.Address), nameof(BuyerOrderResponseDto.DeliveryAddress))]
        [MapProperty(nameof(Order.Status), nameof(BuyerOrderResponseDto.OrderStatus))]
        [MapProperty(new[] { nameof(Order.Product), nameof(Product.Name) }, new[] { nameof(BuyerOrderResponseDto.ProductName) })]
        [MapProperty(new[] { nameof(Order.Product), nameof(Product.Sku) }, new[] { nameof(BuyerOrderResponseDto.ProductSku) })]
        private static partial BuyerOrderResponseDto MapToBuyerOrderDto(Order order);

        [MapProperty(nameof(Order.Id), nameof(SellerOrderResponseDto.OrderId))]
        [MapProperty(nameof(Order.Total), nameof(SellerOrderResponseDto.OrderValue))]
        [MapProperty(nameof(Order.Count), nameof(SellerOrderResponseDto.ProductCount))]
        [MapProperty(nameof(Order.Address), nameof(SellerOrderResponseDto.DeliveryAddress))]
        [MapProperty(new[] { nameof(Order.Product), nameof(Product.Name) }, new[] { nameof(SellerOrderResponseDto.ProductName) })]
        [MapProperty(new[] { nameof(Order.Product), nameof(Product.Sku) }, new[] { nameof(SellerOrderResponseDto.ProductSku) })]
        private static partial SellerOrderResponseDto MapToSellerOrderDto(Order order);
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, AllowNullPropertyAssignment = false)]
    public static partial class ProductUpdateMapper
    {
        [MapperIgnoreTarget(nameof(Product.Id))]
        [MapperIgnoreTarget(nameof(Product.SellerId))]
        [MapperIgnoreTarget(nameof(Product.Seller))]
        [MapperIgnoreTarget(nameof(Product.CountInStock))]
        [MapperIgnoreTarget(nameof(Product.ReservedCount))]
        public static partial void ApplyUpdate(UpdateProductDto source, Product target);
    }
}
