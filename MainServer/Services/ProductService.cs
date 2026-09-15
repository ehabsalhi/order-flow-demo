using MainServer.Data;
using MainServer.DTOs.Common;
using MainServer.DTOs.Products;
using MainServer.Entities;
using MainServer.Exceptions;
using MainServer.Helpers;
using MainServer.Mappings;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Services;

public class ProductService(AppDbContext context, ILogger<ProductService> logger)
{
    public async Task<ApiResponse<PaginationResponse<ProductResponse>>> GetProductsAsync(
        ProductQueryRequest query,
        CancellationToken cancellationToken = default)
    {
        var productsQuery = context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(search) ||
                (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        productsQuery = ApplySorting(productsQuery, query.SortBy, query.SortDirection);

        var projectedQuery = productsQuery.Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Stock,
            p.CategoryId,
            p.Category.Name,
            p.CreatedAt,
            p.UpdatedAt));

        var paged = await projectedQuery.ToPagedResultAsync(query, cancellationToken);
        return new ApiResponse<PaginationResponse<ProductResponse>>(true, paged);
    }

    public async Task<ApiResponse<ProductResponse>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        return new ApiResponse<ProductResponse>(true, product.ToResponse());
    }

    public async Task<ApiResponse<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        await context.Entry(product).Reference(p => p.Category).LoadAsync(cancellationToken);

        logger.LogInformation("Product created with id {ProductId}", product.Id);

        return new ApiResponse<ProductResponse>(true, product.ToResponse());
    }

    public async Task<ApiResponse<ProductResponse>> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        await context.Entry(product).Reference(p => p.Category).LoadAsync(cancellationToken);

        logger.LogInformation("Product updated with id {ProductId}", product.Id);

        return new ApiResponse<ProductResponse>(true, product.ToResponse());
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        var hasOrderItems = await context.OrderItems.AnyAsync(i => i.ProductId == id, cancellationToken);
        if (hasOrderItems)
        {
            throw new BusinessRuleException("Cannot delete a product that is referenced by existing orders.");
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product deleted with id {ProductId}", id);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        var exists = await context.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("Category not found.");
        }
    }

    private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "price" => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
        };
    }
}
