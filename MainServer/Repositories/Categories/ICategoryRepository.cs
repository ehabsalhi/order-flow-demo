using MainServer.DTOs.Common;
using MainServer.Entities;

namespace MainServer.Repositories.Categories;

public interface ICategoryRepository
{
    Task<PaginationResponse<Category>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Category?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);

    Task<bool> HasProductsAsync(int id, CancellationToken cancellationToken = default);

    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);

    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
}
