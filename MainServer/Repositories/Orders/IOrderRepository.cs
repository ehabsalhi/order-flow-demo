using MainServer.DTOs.Common;
using MainServer.DTOs.Orders;
using MainServer.Entities;

namespace MainServer.Repositories.Orders;

public interface IOrderRepository
{
    Task<Order> CreateAsync(
        int userId,
        IReadOnlyList<CreateOrderItemRequest> items,
        CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginationResponse<OrderListResponse>> GetPagedByUserIdAsync(
        int userId,
        PaginationRequest request,
        CancellationToken cancellationToken = default);

    Task<PaginationResponse<OrderListResponse>> GetPagedAsync(
        OrderQueryRequest request,
        CancellationToken cancellationToken = default);
}
