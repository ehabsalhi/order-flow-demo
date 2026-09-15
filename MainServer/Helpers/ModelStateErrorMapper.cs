using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MainServer.Helpers;

public static class ModelStateErrorMapper
{
    public static Dictionary<string, string[]> Map(ModelStateDictionary modelState)
    {
        var entries = modelState.Where(entry => entry.Value?.Errors.Count > 0).ToList();

        var hasJsonFieldErrors = entries.Any(entry =>
            entry.Key.StartsWith("$.", StringComparison.Ordinal)
        );
        var errors = new Dictionary<string, List<string>>();

        foreach (var (key, entry) in entries)
        {
            if (
                hasJsonFieldErrors
                && string.Equals(key, "request", StringComparison.OrdinalIgnoreCase)
            )
            {
                continue;
            }

            var field = ToFieldName(key);

            foreach (var error in entry!.Errors)
            {
                AddError(errors, field, ToMessage(field, error));
            }
        }

        if (errors.Count == 0)
        {
            return new Dictionary<string, string[]>
            {
                ["body"] = ["Request body is invalid or missing."],
            };
        }

        return errors.ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Distinct(StringComparer.Ordinal).ToArray()
        );
    }

    public static string ToFieldName(string key)
    {
        if (key.StartsWith("$.", StringComparison.Ordinal))
        {
            key = key[2..];
        }

        if (string.Equals(key, "request", StringComparison.OrdinalIgnoreCase))
        {
            return "body";
        }

        var property = key.Split('.').Last();

        if (property.Contains('['))
        {
            var bracketIndex = property.IndexOf('[', StringComparison.Ordinal);
            return ToCamelCase(property[..bracketIndex]) + property[bracketIndex..];
        }

        return ToCamelCase(property);
    }

    private static string ToMessage(string field, ModelError error)
    {
        var message = error.ErrorMessage ?? error.Exception?.Message ?? "Invalid value.";

        if (IsJsonConversionError(message))
        {
            var jsonField = ExtractJsonField(message) ?? field;
            return $"{ToLabel(jsonField)} has an invalid value.";
        }

        if (field == "body" && message.Contains("required", StringComparison.OrdinalIgnoreCase))
        {
            return "Request body is invalid or missing.";
        }

        return message;
    }

    private static bool IsJsonConversionError(string message) =>
        message.Contains("could not be converted", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractJsonField(string message)
    {
        const string marker = "Path: $.";
        var start = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return null;
        }

        start += marker.Length;
        var end = start;

        while (end < message.Length && (char.IsLetterOrDigit(message[end]) || message[end] == '_'))
        {
            end++;
        }

        return end > start ? message[start..end] : null;
    }

    private static string ToLabel(string field) =>
        field switch
        {
            _ => field,
        };

    private static void AddError(
        Dictionary<string, List<string>> errors,
        string field,
        string message
    )
    {
        if (!errors.TryGetValue(field, out var messages))
        {
            messages = [];
            errors[field] = messages;
        }

        messages.Add(message);
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
