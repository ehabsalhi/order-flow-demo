using PaymentService.Entities.Enums;

namespace PaymentService.DTOs;

public record PaymentResponse(
    int PaymentId,
    int OrderId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string? TransactionId,
    PaymentProvider Provider,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
