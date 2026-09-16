using System.Globalization;
using FluentValidation;
using Grpc.Core;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Grpc;

public class PaymentGrpcService(
    IPaymentService paymentService,
    IValidator<CreatePaymentRequest> validator
) : Payments.PaymentsBase
{
    public override async Task<PaymentGrpcReply> CreatePayment(
        CreatePaymentGrpcRequest request,
        ServerCallContext context
    )
    {
        if (
            !decimal.TryParse(
                request.Amount,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var amount
            )
        )
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Amount must be a valid decimal number.")
            );
        }

        var dto = new CreatePaymentRequest(request.OrderId, amount, request.Currency);

        await validator.ValidateAndThrowAsync(dto, context.CancellationToken);

        var result = await paymentService.CreatePaymentAsync(dto, context.CancellationToken);
        var payment = result.Data!;

        return new PaymentGrpcReply
        {
            PaymentId = payment.PaymentId,
            OrderId = payment.OrderId,
            Amount = payment.Amount.ToString(CultureInfo.InvariantCulture),
            Currency = payment.Currency,
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId ?? string.Empty,
            Provider = payment.Provider.ToString(),
            CreatedAt = payment.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
            UpdatedAt = payment.UpdatedAt.ToString("O", CultureInfo.InvariantCulture),
        };
    }
}
