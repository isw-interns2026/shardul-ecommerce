namespace ECommerce.Models.DTO.Buyer
{
    public class BuyerCartItemResponseDto
    {
        public string Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int AvailableStock { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int CountInCart { get; set; }
    }
}
