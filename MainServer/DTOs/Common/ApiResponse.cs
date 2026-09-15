using System.Collections.Frozen;

namespace MainServer.DTOs.Common;

public record ApiResponse<T>(bool Success, T? Data);

public record ApiErrorResponse(
    bool Success,
    string Message,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static IReadOnlyDictionary<string, string[]> EmptyErrors { get; } =
        FrozenDictionary<string, string[]>.Empty;
}
