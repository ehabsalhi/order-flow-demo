namespace NotificationService.DTOs.Common;

public class PaginationRequest
{
    private const int MaxPageSize = 100;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int GetNormalizedPage() => Page < 1 ? 1 : Page;

    public int GetNormalizedPageSize()
    {
        if (PageSize < 1)
        {
            return 10;
        }

        return PageSize > MaxPageSize ? MaxPageSize : PageSize;
    }
}
