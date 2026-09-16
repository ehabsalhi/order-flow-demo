using PaymentService.DTOs;
using PaymentService.DTOs.Common;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<ApiResponse<PaymentResponse>> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<ApiResponse<PaginationResponse<PaymentResponse>>> GetPaymentsByOrderIdAsync(
        int orderId,
        PaginationRequest request,
        CancellationToken cancellationToken = default
    );
}
