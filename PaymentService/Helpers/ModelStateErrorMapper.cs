using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PaymentService.Helpers;

public static class ModelStateErrorMapper
{
    public static Dictionary<string, string[]> Map(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => ToFieldName(entry.Key),
                entry =>
                    entry
                        .Value!.Errors.Select(e =>
                            string.IsNullOrWhiteSpace(e.ErrorMessage)
                                ? "Invalid value."
                                : e.ErrorMessage
                        )
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()
            );

        if (errors.Count == 0)
        {
            errors["body"] = ["Request body is invalid or missing."];
        }

        return errors;
    }

    public static string ToFieldName(string key)
    {
        if (key.StartsWith("$.", StringComparison.Ordinal))
        {
            key = key[2..];
        }

        if (
            string.IsNullOrEmpty(key)
            || string.Equals(key, "request", StringComparison.OrdinalIgnoreCase)
        )
        {
            return "body";
        }

        var property = key.Split('.').Last();

        return char.ToLowerInvariant(property[0]) + property[1..];
    }
}
