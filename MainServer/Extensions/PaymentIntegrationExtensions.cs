using MainServer.Messaging;
using MainServer.Services.Payments;
using PaymentsClient = MainServer.Grpc.Payments.PaymentsClient;

namespace MainServer.Extensions;

public static class PaymentIntegrationExtensions
{
    public static IServiceCollection AddPaymentIntegration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var address = configuration["Services:PaymentServiceGrpc"] ?? "http://localhost:3101";

        services.AddGrpcClient<PaymentsClient>(options => options.Address = new Uri(address));

        services.AddScoped<IPaymentGateway, PaymentGrpcGateway>();

        services.Configure<PaymentRabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddHostedService<PaymentEventsConsumer>();

        return services;
    }
}
