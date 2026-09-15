using AutoMapper;
using MainServer.DTOs.Common;
using MainServer.DTOs.Products;
using MainServer.Entities;
using MainServer.Exceptions;
using MainServer.Helpers;
using MainServer.Repositories.Categories;
using MainServer.Repositories.Products;

namespace MainServer.Services.Products;

public class ProductService(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ILogger<ProductService> logger
) : IProductService
{
    public async Task<ApiResponse<PaginationResponse<ProductResponse>>> GetProductsAsync(
        ProductQueryRequest query,
        CancellationToken cancellationToken = default
    )
    {
        var result = await productRepository.GetAllProductsAsync(query, cancellationToken);
        return new ApiResponse<PaginationResponse<ProductResponse>>(
            true,
            mapper.MapPaged<Product, ProductResponse>(result)
        );
    }

    public async Task<ApiResponse<ProductResponse>> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var product =
            await productRepository.GetByIdWithCategoryAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        return new ApiResponse<ProductResponse>(true, mapper.Map<ProductResponse>(product));
    }

    public async Task<ApiResponse<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (!await categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            throw new NotFoundException("Category not found.");
        }

        var now = DateTime.UtcNow;
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        product = await productRepository.AddAsync(product, cancellationToken);

        logger.LogInformation("Product created with id {ProductId}", product.Id);

        return new ApiResponse<ProductResponse>(true, mapper.Map<ProductResponse>(product));
    }

    public async Task<ApiResponse<ProductResponse>> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var product =
            await productRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        if (!await categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
        {
            throw new NotFoundException("Category not found.");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await productRepository.UpdateAsync(product, cancellationToken);

        logger.LogInformation("Product updated with id {ProductId}", product.Id);

        return new ApiResponse<ProductResponse>(true, mapper.Map<ProductResponse>(product));
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product =
            await productRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");

        await productRepository.DeleteAsync(product, cancellationToken);

        logger.LogInformation("Product deleted with id {ProductId}", id);
    }
}
