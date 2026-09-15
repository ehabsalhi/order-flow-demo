using MainServer.DTOs.Common;

namespace MainServer.DTOs.Products;

public class ProductQueryRequest : PaginationRequest
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;
    public SortDirection SortDirection { get; set; } = SortDirection.Asc;
}
