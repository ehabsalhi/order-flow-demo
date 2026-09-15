using MainServer.Data;
using MainServer.DTOs.Common;
using MainServer.DTOs.Products;
using MainServer.Entities;
using MainServer.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Repositories.Products;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<PaginationResponse<Product>> GetAllProductsAsync(
        ProductQueryRequest query,
        CancellationToken cancellationToken = default
    )
    {
        var productsQuery = context.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(search)
                || (p.Description != null && p.Description.ToLower().Contains(search))
            );
        }

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        productsQuery = ApplySorting(productsQuery, query.SortBy, query.SortDirection);

        return await productsQuery.ToPagedResultAsync(query, cancellationToken);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetByIdWithCategoryAsync(
        int id,
        CancellationToken cancellationToken = default
    ) =>
        context
            .Products.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken = default
    ) =>
        context
            .Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<Product>> GetByIdsForUpdateAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    ) => context.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public async Task<Product> AddAsync(
        Product product,
        CancellationToken cancellationToken = default
    )
    {
        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);
        await context.Entry(product).Reference(p => p.Category).LoadAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync(cancellationToken);
        await context.Entry(product).Reference(p => p.Category).LoadAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Product> ApplySorting(
        IQueryable<Product> query,
        ProductSortBy sortBy,
        SortDirection sortDirection
    )
    {
        var descending = sortDirection == SortDirection.Desc;

        return sortBy switch
        {
            ProductSortBy.Price => descending
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            ProductSortBy.CreatedAt => descending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
        };
    }
}
