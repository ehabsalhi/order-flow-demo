namespace PaymentService.Providers;

public record PaymentChargeResult(bool Success, string? TransactionId, string? FailureReason)
{
    public static PaymentChargeResult Approved(string transactionId) =>
        new(true, transactionId, null);

    public static PaymentChargeResult Declined(string reason) => new(false, null, reason);
}
