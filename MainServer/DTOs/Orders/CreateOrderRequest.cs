namespace MainServer.DTOs.Orders;

public record CreateOrderRequest(IReadOnlyList<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(int ProductId, int Quantity);
