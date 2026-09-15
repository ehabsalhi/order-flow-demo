using MainServer.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services
        .AddDatabase(builder.Configuration)
        .AddAuthenticationConfiguration(builder.Configuration)
        .AddSwaggerConfiguration()
        .AddApplicationServices()
        .AddValidation()
        .AddExceptionHandling()
        .AddCorsConfiguration(builder.Configuration);

    var app = builder.Build();

    app.UseApplicationMiddleware();
    await app.SeedDatabaseAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
