namespace MainServer.DTOs.Common;

public record ApiResponse<T>(bool Success, T? Data);

public record ApiErrorResponse(bool Success, string Message, object? Errors);
