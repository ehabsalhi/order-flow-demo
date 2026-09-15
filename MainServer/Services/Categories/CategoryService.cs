using AutoMapper;
using MainServer.DTOs.Categories;
using MainServer.DTOs.Common;
using MainServer.Entities;
using MainServer.Exceptions;
using MainServer.Helpers;
using MainServer.Repositories.Categories;

namespace MainServer.Services.Categories;

public class CategoryService(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<ApiResponse<PaginationResponse<CategoryResponse>>> GetCategoriesAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await categoryRepository.GetPagedAsync(request, cancellationToken);
        return new ApiResponse<PaginationResponse<CategoryResponse>>(true, mapper.MapPaged<Category, CategoryResponse>(paged));
    }

    public async Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        return new ApiResponse<CategoryResponse>(true, mapper.Map<CategoryResponse>(category));
    }

    public async Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await categoryRepository.NameExistsAsync(request.Name, cancellationToken: cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var now = DateTime.UtcNow;
        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        category = await categoryRepository.AddAsync(category, cancellationToken);

        logger.LogInformation("Category created with id {CategoryId}", category.Id);

        return new ApiResponse<CategoryResponse>(true, mapper.Map<CategoryResponse>(category));
    }

    public async Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (await categoryRepository.NameExistsAsync(request.Name, id, cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.UpdatedAt = DateTime.UtcNow;

        await categoryRepository.UpdateAsync(category, cancellationToken);

        logger.LogInformation("Category updated with id {CategoryId}", category.Id);

        return new ApiResponse<CategoryResponse>(true, mapper.Map<CategoryResponse>(category));
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        if (await categoryRepository.HasProductsAsync(id, cancellationToken))
        {
            throw new BusinessRuleException("Cannot delete a category that contains products.");
        }

        await categoryRepository.DeleteAsync(category, cancellationToken);

        logger.LogInformation("Category deleted with id {CategoryId}", id);
    }
}
