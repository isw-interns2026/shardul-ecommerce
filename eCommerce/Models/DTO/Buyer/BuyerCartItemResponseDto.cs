namespace ECommerce.Models.DTO.Buyer
{
    public class BuyerCartItemResponseDto
    {
        public required Guid Id { get; set; }
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required int AvailableStock { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int CountInCart { get; set; }
    }
}
