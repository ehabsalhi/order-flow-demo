namespace PaymentService.DTOs;

public record CreatePaymentRequest(int OrderId, decimal Amount, string Currency);
