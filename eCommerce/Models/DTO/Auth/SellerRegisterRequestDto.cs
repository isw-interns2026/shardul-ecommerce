namespace ECommerce.Models.DTO.Auth
{
    public class SellerRegisterRequestDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Ban { get; set; }
    }
}
