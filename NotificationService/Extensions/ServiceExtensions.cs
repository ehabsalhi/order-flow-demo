using NotificationService.Providers;
using NotificationService.Services;

namespace NotificationService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHealthChecks();
        services.AddScoped<INotificationProvider, MockNotificationProvider>();
        services.AddScoped<INotificationService, Services.NotificationService>();
        return services;
    }
}
