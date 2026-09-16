using Microsoft.EntityFrameworkCore;
using NotificationService.DTOs.Common;

namespace NotificationService.Helpers;

public static class PaginationHelper
{
    public static async Task<PaginationResponse<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var page = request.GetNormalizedPage();
        var pageSize = request.GetNormalizedPageSize();

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<T>(items, page, pageSize, totalCount, totalPages);
    }
}
