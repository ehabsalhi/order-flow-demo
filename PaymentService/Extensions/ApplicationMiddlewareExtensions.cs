using PaymentService.Grpc;
using Serilog;

namespace PaymentService.Extensions;

public static class ApplicationMiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler(_ => { });
        app.UseSerilogRequestLogging();
        app.MapControllers();
        app.MapGrpcService<PaymentGrpcService>();
        app.MapHealthChecks("/health");

        return app;
    }
}
