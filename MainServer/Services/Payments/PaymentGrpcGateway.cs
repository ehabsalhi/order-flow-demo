using System.Globalization;
using MainServer.Grpc;
using PaymentsClient = MainServer.Grpc.Payments.PaymentsClient;

namespace MainServer.Services.Payments;

public class PaymentGrpcGateway(PaymentsClient client) : IPaymentGateway
{
    public async Task<PaymentResult> ChargeOrderAsync(
        int orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default
    )
    {
        var reply = await client.CreatePaymentAsync(
            new CreatePaymentGrpcRequest
            {
                OrderId = orderId,
                Amount = amount.ToString(CultureInfo.InvariantCulture),
                Currency = currency,
            },
            cancellationToken: cancellationToken
        );

        return new PaymentResult(reply.PaymentId, reply.Status, reply.TransactionId);
    }
}
