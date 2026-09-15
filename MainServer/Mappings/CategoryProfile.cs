using AutoMapper;
using MainServer.DTOs.Categories;
using MainServer.Entities;

namespace MainServer.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryResponse>();
    }
}
