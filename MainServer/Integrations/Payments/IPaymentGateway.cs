namespace MainServer.Integrations.Payments;

public record PaymentResult(int PaymentId, string Status, string? TransactionId);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeOrderAsync(
        int orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default
    );
}
