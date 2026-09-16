using MainServer.DTOs.Common;
using MainServer.DTOs.Payments;
using MainServer.Exceptions;
using MainServer.Integrations.Payments;
using MainServer.Repositories.Orders;
using MainServer.Services.Auth;

namespace MainServer.Services.Payments;

public class PaymentService(
    IPaymentHttpGateway paymentHttpGateway,
    IOrderRepository orderRepository,
    ICurrentUserService currentUser) : IPaymentService
{
    public async Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var payment = await paymentHttpGateway.GetPaymentByIdAsync(id, cancellationToken);
        await EnsureCanAccessOrderAsync(payment.OrderId, cancellationToken);

        return new ApiResponse<PaymentResponse>(true, payment);
    }

    public async Task<ApiResponse<PaginationResponse<PaymentResponse>>> GetPaymentsByOrderIdAsync(
        int orderId,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCanAccessOrderAsync(orderId, cancellationToken);

        var paged = await paymentHttpGateway.GetPaymentsByOrderIdAsync(
            orderId,
            request,
            cancellationToken);

        return new ApiResponse<PaginationResponse<PaymentResponse>>(true, paged);
    }

    private async Task EnsureCanAccessOrderAsync(int orderId, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
        {
            throw new ForbiddenException("You do not have access to this order.");
        }
    }
}
