using MainServer.DTOs.Common;
using MainServer.DTOs.Payments;

namespace MainServer.Services.Payments;

public interface IPaymentService
{
    Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaginationResponse<PaymentResponse>>> GetPaymentsByOrderIdAsync(
        int orderId,
        PaginationRequest request,
        CancellationToken cancellationToken = default);
}
