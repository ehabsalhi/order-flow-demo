using MainServer.Data;
using MainServer.DTOs.Common;
using MainServer.Entities;
using MainServer.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Repositories.Categories;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<PaginationResponse<Category>> GetPagedAsync(
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name);

        return await query.ToPagedResultAsync(request, cancellationToken);
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Category?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        context.Categories.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return context.Categories.AnyAsync(
            c => c.Name.ToLower() == normalizedName && (excludeId == null || c.Id != excludeId),
            cancellationToken);
    }

    public Task<bool> HasProductsAsync(int id, CancellationToken cancellationToken = default) =>
        context.Products.AnyAsync(p => p.CategoryId == id, cancellationToken);

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        context.Categories.Update(category);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
