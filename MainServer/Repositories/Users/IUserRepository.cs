using MainServer.Entities;

namespace MainServer.Repositories.Users;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
}
