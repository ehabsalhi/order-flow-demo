using MainServer.DTOs.Common;
using MainServer.DTOs.Payments;

namespace MainServer.Integrations.Payments;

public interface IPaymentHttpGateway
{
    Task<PaymentResponse> GetPaymentByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PaginationResponse<PaymentResponse>> GetPaymentsByOrderIdAsync(
        int orderId,
        PaginationRequest request,
        CancellationToken cancellationToken = default);
}
