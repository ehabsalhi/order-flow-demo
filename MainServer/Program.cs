using MainServer.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
);

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
var builderConfiguration = builder.Configuration;
builder
    .Services.AddDatabase(builderConfiguration)
    .AddAuthenticationConfiguration(builderConfiguration)
    .AddSwaggerConfiguration()
    .AddApplicationServices()
    .AddValidation()
    .AddExceptionHandling()
    .AddCorsConfiguration(builderConfiguration)
    .AddRateLimiting(builderConfiguration);

var app = builder.Build();

app.UseApplicationMiddleware();
await app.SeedDatabaseAsync();

app.Run();
