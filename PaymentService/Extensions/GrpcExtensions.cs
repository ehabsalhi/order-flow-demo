using PaymentService.Grpc;

namespace PaymentService.Extensions;

public static class GrpcExtensions
{
    public static IServiceCollection AddGrpcConfiguration(this IServiceCollection services)
    {
        services.AddGrpc(options => options.Interceptors.Add<GrpcExceptionInterceptor>());

        return services;
    }
}
