using PaymentService.Extensions;
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

var configuration = builder.Configuration;
builder
    .Services.AddDatabase(configuration)
    .AddApplicationServices()
    .AddGrpcConfiguration()
    .AddMessaging(configuration)
    .AddValidation()
    .AddExceptionHandling();

var app = builder.Build();

app.UseApplicationMiddleware();
await app.MigrateDatabaseAsync();

app.Run();
