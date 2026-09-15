using MainServer.DTOs.Categories;
using MainServer.DTOs.Common;

namespace MainServer.Services.Categories;

public interface ICategoryService
{
    Task<ApiResponse<PaginationResponse<CategoryResponse>>> GetCategoriesAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
