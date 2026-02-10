namespace ECommerce.Models.DTO.Buyer
{
    public class BuyerProductResponseDto
    {
        public string Id { get; set; }
        public string Sku { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int AvailableStock { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
    }
}
