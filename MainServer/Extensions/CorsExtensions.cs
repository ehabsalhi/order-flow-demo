namespace MainServer.Extensions;

public static class CorsExtensions
{
    public const string DevelopmentPolicy = "DevelopmentCors";

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentPolicy, policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
