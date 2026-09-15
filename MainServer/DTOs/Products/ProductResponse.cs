namespace MainServer.DTOs.Products;

public record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    int CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt);
