using MainServer.Data;
using MainServer.DTOs.Categories;
using MainServer.DTOs.Common;
using MainServer.Entities;
using MainServer.Exceptions;
using MainServer.Helpers;
using MainServer.Mappings;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Services;

public class CategoryService(AppDbContext context, ILogger<CategoryService> logger)
{
    public async Task<ApiResponse<PaginationResponse<CategoryResponse>>> GetCategoriesAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description, c.CreatedAt, c.UpdatedAt));

        var paged = await query.ToPagedResultAsync(request, cancellationToken);
        return new ApiResponse<PaginationResponse<CategoryResponse>>(true, paged);
    }

    public async Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
        {
            throw new NotFoundException("Category not found.");
        }

        return new ApiResponse<CategoryResponse>(true, category.ToResponse());
    }

    public async Task<ApiResponse<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureUniqueNameAsync(request.Name, null, cancellationToken);

        var now = DateTime.UtcNow;
        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category created with id {CategoryId}", category.Id);

        return new ApiResponse<CategoryResponse>(true, category.ToResponse());
    }

    public async Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(
        int id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category not found.");
        }

        await EnsureUniqueNameAsync(request.Name, id, cancellationToken);

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category updated with id {CategoryId}", category.Id);

        return new ApiResponse<CategoryResponse>(true, category.ToResponse());
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category not found.");
        }

        var hasProducts = await context.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);
        if (hasProducts)
        {
            throw new BusinessRuleException("Cannot delete a category that contains products.");
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Category deleted with id {CategoryId}", id);
    }

    private async Task EnsureUniqueNameAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        var exists = await context.Categories.AnyAsync(
            c => c.Name.ToLower() == normalizedName && (excludeId == null || c.Id != excludeId),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException("A category with this name already exists.");
        }
    }
}
