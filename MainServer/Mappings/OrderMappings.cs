using MainServer.DTOs.Orders;
using MainServer.Entities;

namespace MainServer.Mappings;

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) =>
        new(
            order.Id,
            order.UserId,
            order.User.Email,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => i.ToResponse()).ToList());

    public static OrderItemResponse ToResponse(this OrderItem item) =>
        new(item.Id, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.UnitPrice * item.Quantity);
}
