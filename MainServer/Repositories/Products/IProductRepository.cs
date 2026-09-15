using MainServer.DTOs.Common;
using MainServer.DTOs.Products;
using MainServer.Entities;

namespace MainServer.Repositories.Products;

public interface IProductRepository
{
    Task<PaginationResponse<Product>> GetAllProductsAsync(
        ProductQueryRequest query,
        CancellationToken cancellationToken = default
    );

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken cancellationToken = default);

    Task<Product?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task<List<Product>> GetByIdsForUpdateAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );

    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);

    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);

    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
}
