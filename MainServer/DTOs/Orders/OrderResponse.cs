using MainServer.Entities.Enums;

namespace MainServer.DTOs.Orders;

public record OrderResponse(
    int Id,
    int UserId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<OrderItemResponse> Items);

public record OrderListResponse(
    int Id,
    int UserId,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ItemCount);

public record OrderItemResponse(
    int Id,
    int ProductId,
    decimal Price,
    int Quantity,
    decimal LineTotal);
