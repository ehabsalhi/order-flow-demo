using NotificationService.Messaging;

namespace NotificationService.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
        services.AddHostedService<PaymentEventsConsumer>();
        return services;
    }
}
