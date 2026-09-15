using MainServer.Data;
using MainServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Repositories.Users;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default
    ) => context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    ) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return user;
    }
}
