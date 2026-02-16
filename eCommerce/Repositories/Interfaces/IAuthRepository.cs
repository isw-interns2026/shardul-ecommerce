using ECommerce.Models.Domain.Entities;

namespace ECommerce.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        void CreateBuyer(Buyer buyer);
        void CreateSeller(Seller seller);
        Task<Buyer?> GetBuyerIfValidCredentialsAsync(string email, string password);
        Task<Seller?> GetSellerIfValidCredentialsAsync(string email, string password);
        Task<Buyer> GetBuyerByIdAsync(Guid buyerId);
    }
}
