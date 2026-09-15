using MainServer.DTOs.Categories;
using MainServer.Entities;

namespace MainServer.Mappings;

public static class CategoryMappings
{
    public static CategoryResponse ToResponse(this Category category) =>
        new(category.Id, category.Name, category.Description, category.CreatedAt, category.UpdatedAt);
}
