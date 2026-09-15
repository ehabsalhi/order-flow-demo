using System.Net;
using FluentValidation;
using MainServer.DTOs.Common;
using MainServer.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace MainServer.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message, errors) = MapException(exception);

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception occurred.");
        }
        else if (exception is not ValidationException)
        {
            logger.LogWarning(exception, "Handled application exception: {Message}", message);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(false, message, errors);
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private (int StatusCode, string Message, object? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                "Validation failed.",
                validationException.Errors
                    .GroupBy(e => ToCamelCase(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),

            NotFoundException notFound => ((int)HttpStatusCode.NotFound, notFound.Message, Array.Empty<string>()),
            ConflictException conflict => ((int)HttpStatusCode.Conflict, conflict.Message, Array.Empty<string>()),
            BusinessRuleException businessRule => ((int)HttpStatusCode.UnprocessableEntity, businessRule.Message, Array.Empty<string>()),
            UnauthorizedAppException unauthorized => ((int)HttpStatusCode.Unauthorized, unauthorized.Message, Array.Empty<string>()),
            ForbiddenException forbidden => ((int)HttpStatusCode.Forbidden, forbidden.Message, Array.Empty<string>()),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.",
                Array.Empty<string>())
        };
    }

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        return char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }
}
