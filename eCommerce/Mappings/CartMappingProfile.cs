using AutoMapper;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Buyer;

namespace ECommerce.Mappings
{
    public class CartMappingProfile : Profile
    {
        public CartMappingProfile()
        {
            CreateMap<Product, BuyerCartItemResponseDto>();
        }
    }
}
