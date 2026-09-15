using AutoMapper;
using MainServer.DTOs.Products;
using MainServer.Entities;

namespace MainServer.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
    }
}
