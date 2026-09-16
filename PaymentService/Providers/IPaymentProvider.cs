using PaymentService.Entities.Enums;

namespace PaymentService.Providers;

public interface IPaymentProvider
{
    PaymentProvider Provider { get; }

    Task<PaymentChargeResult> ChargeAsync(
        int orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default
    );
}
