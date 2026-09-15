using MainServer.DTOs.Common;
using MainServer.DTOs.Orders;

namespace MainServer.Services.Orders;

public interface IOrderService
{
    Task<ApiResponse<OrderResponse>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResponse<PaginationResponse<OrderListResponse>>> GetMyOrdersAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaginationResponse<OrderListResponse>>> GetAdminOrdersAsync(
        OrderQueryRequest request,
        CancellationToken cancellationToken = default);
}
