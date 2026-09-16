using Microsoft.OpenApi.Models;

namespace PaymentService.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "OrderFlow Payment Service API",
                    Version = "v1",
                    Description = "Independent microservice that owns payment processing. ",
                }
            );
        });

        return services;
    }
}
