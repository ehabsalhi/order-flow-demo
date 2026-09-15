using AutoMapper;
using MainServer.DTOs.Common;
using MainServer.DTOs.Orders;
using MainServer.Exceptions;
using MainServer.Repositories.Orders;
using MainServer.Services.Auth;

namespace MainServer.Services.Orders;

public class OrderService(
    IOrderRepository orderRepository,
    ICurrentUserService currentUser,
    IMapper mapper,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.CreateAsync(currentUser.UserId, request.Items, cancellationToken);

        logger.LogInformation("Order {OrderId} created for user {UserId}", order.Id, currentUser.UserId);

        return new ApiResponse<OrderResponse>(true, mapper.Map<OrderResponse>(order));
    }

    public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not have access to this order.");
        }

        return new ApiResponse<OrderResponse>(true, mapper.Map<OrderResponse>(order));
    }

    public async Task<ApiResponse<PaginationResponse<OrderListResponse>>> GetMyOrdersAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await orderRepository.GetPagedByUserIdAsync(currentUser.UserId, request, cancellationToken);
        return new ApiResponse<PaginationResponse<OrderListResponse>>(true, paged);
    }

    public async Task<ApiResponse<PaginationResponse<OrderListResponse>>> GetAdminOrdersAsync(
        OrderQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await orderRepository.GetPagedAsync(request, cancellationToken);
        return new ApiResponse<PaginationResponse<OrderListResponse>>(true, paged);
    }
}
