using AutoMapper;
using ECommerce.Models.Domain.Entities;
using ECommerce.Models.DTO.Buyer;
using ECommerce.Models.DTO.Seller;

namespace ECommerce.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<UpdateProductDto, Product>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Product, SellerProductResponseDto>();

            CreateMap<Product, BuyerProductResponseDto>();

            CreateMap<AddProductDto, Product>();
        }
    }
}
