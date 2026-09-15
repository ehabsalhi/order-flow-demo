using FluentValidation;
using FluentValidation.AspNetCore;
using MainServer.DTOs.Common;
using MainServer.Helpers;
using MainServer.Validators;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = ModelStateErrorMapper.Map(context.ModelState);
                return new BadRequestObjectResult(new ApiErrorResponse(false, "Validation failed.", errors));
            };
        });

        return services;
    }
}
