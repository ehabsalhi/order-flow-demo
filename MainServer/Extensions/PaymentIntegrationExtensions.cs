using MainServer.Integrations.Payments;
using MainServer.Messaging;
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

        var grpcAddress = configuration["Services:PaymentServiceGrpc"] ?? "http://localhost:3101";
        var httpAddress = configuration["Services:PaymentServiceHttp"] ?? "http://localhost:3100";

        services.AddGrpcClient<PaymentsClient>(options => options.Address = new Uri(grpcAddress));
        services.AddHttpClient<IPaymentHttpGateway, PaymentHttpGateway>(client =>
        {
            client.BaseAddress = new Uri(httpAddress.TrimEnd('/') + "/");
        });

        services.AddScoped<IPaymentGateway, PaymentGrpcGateway>();

        services.Configure<PaymentRabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddHostedService<PaymentEventsConsumer>();

        return services;
    }
}
