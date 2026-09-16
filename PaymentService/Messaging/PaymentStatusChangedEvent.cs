namespace PaymentService.Messaging;

public record PaymentStatusChangedEvent(
    int PaymentId,
    int OrderId,
    string Status,
    string? TransactionId,
    decimal Amount,
    string Currency,
    string Provider,
    DateTime OccurredAt
)
{
    public string RoutingKey => $"payment.{Status.ToLowerInvariant()}";
}
