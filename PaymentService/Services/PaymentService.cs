using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.DTOs.Common;
using PaymentService.Entities;
using PaymentService.Entities.Enums;
using PaymentService.Exceptions;
using PaymentService.Helpers;
using PaymentService.Messaging;
using PaymentService.Providers;

namespace PaymentService.Services;

public class PaymentService(
    PaymentDbContext context,
    IPaymentProvider paymentProvider,
    ILogger<PaymentService> logger
) : IPaymentService
{
    public async Task<ApiResponse<PaymentResponse>> CreatePaymentAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var orderAlreadyPaid = await context.Payments.AnyAsync(
            p => p.OrderId == request.OrderId && p.Status == PaymentStatus.Succeeded,
            cancellationToken
        );

        if (orderAlreadyPaid)
        {
            throw new ConflictException(
                $"Order {request.OrderId} has already been paid successfully."
            );
        }

        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            Status = PaymentStatus.Pending,
            Provider = paymentProvider.Provider,
            CreatedAt = now,
            UpdatedAt = now,
        };

        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);

        var result = await paymentProvider.ChargeAsync(
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            cancellationToken
        );

        payment.Status = result.Success ? PaymentStatus.Succeeded : PaymentStatus.Failed;
        payment.TransactionId = result.TransactionId;
        payment.UpdatedAt = DateTime.UtcNow;

        context.OutboxMessages.Add(BuildStatusChangedMessage(payment));

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Payment {PaymentId} for order {OrderId} finished with status {Status}.",
            payment.Id,
            payment.OrderId,
            payment.Status
        );

        return new ApiResponse<PaymentResponse>(true, ToResponse(payment));
    }

    public async Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var payment =
            await context
                .Payments.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Payment with id {id} was not found.");

        return new ApiResponse<PaymentResponse>(true, ToResponse(payment));
    }

    public async Task<ApiResponse<PaginationResponse<PaymentResponse>>> GetPaymentsByOrderIdAsync(
        int orderId,
        PaginationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = context
            .Payments.AsNoTracking()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentResponse(
                p.Id,
                p.OrderId,
                p.Amount,
                p.Currency,
                p.Status,
                p.TransactionId,
                p.Provider,
                p.CreatedAt,
                p.UpdatedAt
            ));

        var paged = await query.ToPagedResultAsync(request, cancellationToken);

        return new ApiResponse<PaginationResponse<PaymentResponse>>(true, paged);
    }

    private static PaymentResponse ToResponse(Payment payment) =>
        new(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.TransactionId,
            payment.Provider,
            payment.CreatedAt,
            payment.UpdatedAt
        );

    private static OutboxMessage BuildStatusChangedMessage(Payment payment)
    {
        var statusEvent = new PaymentStatusChangedEvent(
            payment.Id,
            payment.OrderId,
            payment.Status.ToString(),
            payment.TransactionId,
            payment.Amount,
            payment.Currency,
            payment.Provider.ToString(),
            payment.UpdatedAt
        );

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = statusEvent.RoutingKey,
            Payload = JsonSerializer.Serialize(statusEvent),
            CreatedAt = DateTime.UtcNow,
        };
    }
}
