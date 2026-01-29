namespace ECommerce.Models.DTO.Buyer
{
    public class BuyerProductResponseDto
    {
        public required string Id { get; set; }
        public required string Sku { get; set; }

        public required string Name { get; set; }

        public required decimal Price { get; set; }

        public required int CountInStock { get; set; }

        public string? Description { get; set; }

        public byte[]? Images { get; set; }

        public required bool IsListed { get; set; }
    }
}
