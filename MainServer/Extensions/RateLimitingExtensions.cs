using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using MainServer.DTOs.Common;
using MainServer.Helpers;
using Microsoft.AspNetCore.RateLimiting;

namespace MainServer.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var settings =
            configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
            ?? throw new InvalidOperationException("RateLimiting configuration is missing.");

        Validate(settings);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, cancellationToken) =>
                WriteTooManyRequestsResponse(context, settings, cancellationToken);

            options.AddPolicy(
                RateLimitPolicies.Auth,
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetClientIp(httpContext),
                        _ => CreateWindowOptions(settings.Auth)
                    )
            );

            options.AddPolicy(
                RateLimitPolicies.Api,
                httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetApiPartitionKey(httpContext),
                        _ => CreateWindowOptions(settings.Api)
                    )
            );
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions CreateWindowOptions(
        RateLimitPolicySettings policy
    ) =>
        new()
        {
            PermitLimit = policy.PermitLimit,
            Window = TimeSpan.FromMinutes(policy.WindowMinutes),
            QueueLimit = 0,
            AutoReplenishment = true,
        };

    private static async ValueTask WriteTooManyRequestsResponse(
        OnRejectedContext context,
        RateLimitingSettings settings,
        CancellationToken cancellationToken
    )
    {
        var retryAfter = TimeSpan.FromMinutes(
            Math.Max(settings.Auth.WindowMinutes, settings.Api.WindowMinutes)
        );

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata))
        {
            retryAfter = retryAfterMetadata;
        }

        var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(
            CultureInfo.InvariantCulture
        );
        context.HttpContext.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            false,
            "Too many requests. Please try again later.",
            ApiErrorResponse.EmptyErrors
        );

        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    }

    private static string GetApiPartitionKey(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId =
                httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        return $"ip:{GetClientIp(httpContext)}";
    }

    private static string GetClientIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static void Validate(RateLimitingSettings settings)
    {
        if (!IsValid(settings.Auth) || !IsValid(settings.Api))
        {
            throw new InvalidOperationException("RateLimiting configuration is invalid.");
        }
    }

    private static bool IsValid(RateLimitPolicySettings policy) =>
        policy.PermitLimit > 0 && policy.WindowMinutes > 0;
}
