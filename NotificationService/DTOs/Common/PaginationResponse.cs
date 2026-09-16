namespace NotificationService.DTOs.Common;

public record PaginationResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
