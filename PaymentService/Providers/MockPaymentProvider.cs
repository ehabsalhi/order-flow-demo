using PaymentService.Entities.Enums;

namespace PaymentService.Providers;

public class MockPaymentProvider(ILogger<MockPaymentProvider> logger) : IPaymentProvider
{
    private const decimal DeclineAboveAmount = 10_000m;

    public PaymentProvider Provider => PaymentProvider.Mock;

    public async Task<PaymentChargeResult> ChargeAsync(
        int orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default
    )
    {
        await Task.Delay(50, cancellationToken);

        if (amount > DeclineAboveAmount)
        {
            logger.LogWarning(
                "Mock provider declined order {OrderId}: amount {Amount} {Currency} exceeds limit {Limit}.",
                orderId,
                amount,
                currency,
                DeclineAboveAmount
            );

            return PaymentChargeResult.Declined(
                $"Amount exceeds the mock provider limit of {DeclineAboveAmount:N2}."
            );
        }

        var transactionId = $"TXN-{Guid.NewGuid():N}"[..14].ToUpperInvariant();

        logger.LogInformation(
            "Mock provider approved order {OrderId} with transaction {TransactionId}.",
            orderId,
            transactionId
        );

        return PaymentChargeResult.Approved(transactionId);
    }
}
