using Serilog;

namespace NotificationService.Extensions;

public static class ApplicationMiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler(_ => { });
        app.UseSerilogRequestLogging();
        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
