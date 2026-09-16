namespace NotificationService.Messaging;

public record PaymentStatusChangedEvent(
    int PaymentId,
    int OrderId,
    string Status,
    string? TransactionId,
    decimal Amount,
    string Currency,
    string Provider,
    DateTime OccurredAt);
