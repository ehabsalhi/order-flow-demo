using MainServer.Integrations.Notifications;

namespace MainServer.Extensions;

public static class NotificationIntegrationExtensions
{
    public static IServiceCollection AddNotificationIntegration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var httpAddress = configuration["Services:NotificationServiceHttp"] ?? "http://localhost:3200";

        services.AddHttpClient<INotificationHttpGateway, NotificationHttpGateway>(client =>
        {
            client.BaseAddress = new Uri(httpAddress.TrimEnd('/') + "/");
        });

        return services;
    }
}
