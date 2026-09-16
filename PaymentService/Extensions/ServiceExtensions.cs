using PaymentService.Providers;

namespace PaymentService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHealthChecks();
        services.AddScoped<IPaymentProvider, MockPaymentProvider>();
        services.AddScoped<Services.IPaymentService, Services.PaymentService>();

        return services;
    }
}
