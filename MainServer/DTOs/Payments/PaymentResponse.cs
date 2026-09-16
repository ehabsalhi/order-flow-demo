namespace MainServer.DTOs.Payments;

public record PaymentResponse(
    int PaymentId,
    int OrderId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string? TransactionId,
    PaymentProvider Provider,
    DateTime CreatedAt,
    DateTime UpdatedAt);
