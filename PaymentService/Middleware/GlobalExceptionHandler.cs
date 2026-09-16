using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using PaymentService.DTOs.Common;
using PaymentService.Exceptions;
using PaymentService.Helpers;

namespace PaymentService.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
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

    private (
        int StatusCode,
        string Message,
        IReadOnlyDictionary<string, string[]> Errors
    ) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                "Validation failed.",
                validationException
                    .Errors.GroupBy(e => ModelStateErrorMapper.ToFieldName(e.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ),

            NotFoundException notFound => (
                (int)HttpStatusCode.NotFound,
                notFound.Message,
                ApiErrorResponse.EmptyErrors
            ),

            ConflictException conflict => (
                (int)HttpStatusCode.Conflict,
                conflict.Message,
                ApiErrorResponse.EmptyErrors
            ),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                ApiErrorResponse.EmptyErrors
            ),
        };
    }
}
