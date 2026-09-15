using MainServer.DTOs.Common;
using MainServer.DTOs.Products;

namespace MainServer.Services.Products;

public interface IProductService
{
    Task<ApiResponse<PaginationResponse<ProductResponse>>> GetProductsAsync(
        ProductQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ProductResponse>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResponse<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ProductResponse>> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
