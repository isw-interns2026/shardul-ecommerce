namespace ECommerce.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJWT(string userId, string email, string name, string role);
    }
}
