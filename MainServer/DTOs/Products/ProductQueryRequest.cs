using MainServer.DTOs.Common;

namespace MainServer.DTOs.Products;

public class ProductQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
