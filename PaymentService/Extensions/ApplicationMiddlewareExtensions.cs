using Serilog;

namespace PaymentService.Extensions;

public static class ApplicationMiddlewareExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler(_ => { });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "OrderFlow Payment Service API v1"
                );
            });
        }

        app.UseSerilogRequestLogging();
        app.MapControllers();

        return app;
    }
}
