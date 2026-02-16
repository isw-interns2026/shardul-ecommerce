namespace ECommerce.Models.DTO.Seller
{
    public class SellerProductResponseDto
    {
        public required Guid Id { get; set; }
        public required string Sku { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required int CountInStock { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public required bool IsListed { get; set; }
    }
}
