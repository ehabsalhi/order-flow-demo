namespace MainServer.DTOs.Products;

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    int CategoryId);
