using MainServer.Entities.Enums;

namespace MainServer.DTOs.Orders;

public record OrderResponse(
    int Id,
    int UserId,
    string UserEmail,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderItemResponse> Items);

public record OrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
