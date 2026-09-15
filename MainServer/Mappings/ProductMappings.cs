using MainServer.DTOs.Products;
using MainServer.Entities;

namespace MainServer.Mappings;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CategoryId,
            product.Category.Name,
            product.CreatedAt,
            product.UpdatedAt);
}
