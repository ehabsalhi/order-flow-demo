using MainServer.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MainServer.Helpers;

public sealed class SwaggerFilter : ISchemaFilter, IOperationFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (typeof(ProblemDetails).IsAssignableFrom(context.Type))
        {
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            return;
        }

        if (context.Type == typeof(ApiErrorResponse))
        {
            schema.AdditionalPropertiesAllowed = false;
            schema.AdditionalProperties = null;
            schema.Example = new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(false),
                ["message"] = new OpenApiString("Validation failed."),
                ["errors"] = new OpenApiObject
                {
                    ["fieldName"] = new OpenApiArray { new OpenApiString("Error message.") }
                }
            };
            return;
        }

        if (schema.AdditionalProperties is not null)
        {
            schema.Example ??= new OpenApiObject();
        }
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var (statusCode, response) in operation.Responses)
        {
            if (!int.TryParse(statusCode, out var code) || code < 400)
            {
                continue;
            }

            var schema = context.SchemaGenerator.GenerateSchema(
                typeof(ApiErrorResponse),
                context.SchemaRepository);

            response.Content.Clear();
            response.Content["application/json"] = new OpenApiMediaType { Schema = schema };
        }
    }
}
