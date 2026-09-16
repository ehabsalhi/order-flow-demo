using AutoMapper;
using MainServer.DTOs.Common;
using MainServer.DTOs.Orders;
using MainServer.Entities.Enums;
using MainServer.Exceptions;
using MainServer.Repositories.Orders;
using MainServer.Services.Auth;
using MainServer.Integrations.Payments;

namespace MainServer.Services.Orders;

public class OrderService(
    IOrderRepository orderRepository,
    IPaymentGateway paymentGateway,
    ICurrentUserService currentUser,
    IMapper mapper,
    ILogger<OrderService> logger
) : IOrderService
{
    private const string PaymentCurrency = "USD";

    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var order = await orderRepository.CreateAsync(
            currentUser.UserId,
            request.Items,
            cancellationToken
        );

        logger.LogInformation(
            "Order {OrderId} created for user {UserId}",
            order.Id,
            currentUser.UserId
        );

        await ChargeOrderAsync(order, cancellationToken);

        return new ApiResponse<OrderResponse>(true, mapper.Map<OrderResponse>(order));
    }

    private async Task ChargeOrderAsync(Entities.Order order, CancellationToken cancellationToken)
    {
        PaymentResult payment;
        try
        {
            payment = await paymentGateway.ChargeOrderAsync(
                order.Id,
                order.TotalAmount,
                PaymentCurrency,
                cancellationToken
            );
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to initiate payment for order {OrderId}", order.Id);
            return;
        }

        logger.LogInformation(
            "Payment {PaymentId} for order {OrderId} returned status {Status}",
            payment.PaymentId,
            order.Id,
            payment.Status
        );

        var mappedStatus = MapPaymentStatus(payment.Status);
        if (
            mappedStatus is { } status
            && await orderRepository.SetStatusAsync(order.Id, status, cancellationToken)
        )
        {
            order.Status = status;
        }
    }

    private static OrderStatus? MapPaymentStatus(string paymentStatus) =>
        paymentStatus switch
        {
            "Succeeded" => OrderStatus.Paid,
            "Failed" => OrderStatus.Cancelled,
            _ => null,
        };

    public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var order =
            await orderRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not have access to this order.");
        }

        return new ApiResponse<OrderResponse>(true, mapper.Map<OrderResponse>(order));
    }

    public async Task<ApiResponse<PaginationResponse<OrderListResponse>>> GetMyOrdersAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var paged = await orderRepository.GetPagedByUserIdAsync(
            currentUser.UserId,
            request,
            cancellationToken
        );
        return new ApiResponse<PaginationResponse<OrderListResponse>>(true, paged);
    }

    public async Task<ApiResponse<PaginationResponse<OrderListResponse>>> GetAdminOrdersAsync(
        OrderQueryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var paged = await orderRepository.GetPagedAsync(request, cancellationToken);
        return new ApiResponse<PaginationResponse<OrderListResponse>>(true, paged);
    }
}
