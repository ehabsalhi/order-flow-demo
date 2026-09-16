using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using NotificationService.DTOs.Common;
using NotificationService.Exceptions;

namespace NotificationService.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => ((int)HttpStatusCode.NotFound, notFound.Message),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                environment.IsDevelopment() ? exception.Message : "An unexpected error occurred."
            ),
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception occurred.");
        }
        else
        {
            logger.LogWarning(exception, "Handled application exception: {Message}", message);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new ApiErrorResponse(false, message, ApiErrorResponse.EmptyErrors),
            cancellationToken);

        return true;
    }
}
