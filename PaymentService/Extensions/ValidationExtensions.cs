using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs.Common;
using PaymentService.Helpers;
using PaymentService.Validators;

namespace PaymentService.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreatePaymentRequestValidator>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = ModelStateErrorMapper.Map(context.ModelState);
                return new BadRequestObjectResult(
                    new ApiErrorResponse(false, "Validation failed.", errors)
                );
            };
        });

        return services;
    }
}
