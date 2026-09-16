using Serilog;

namespace MainServer.Extensions;

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
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderFlow Main Server API v1");
                options.ConfigObject.PersistAuthorization = true;
            });
        }

        app.UseSerilogRequestLogging();
        app.UseCors(CorsExtensions.DevelopmentPolicy);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();

        return app;
    }
}
