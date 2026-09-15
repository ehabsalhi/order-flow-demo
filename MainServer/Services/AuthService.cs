using MainServer.Data;
using MainServer.DTOs.Auth;
using MainServer.DTOs.Common;
using MainServer.Entities;
using MainServer.Entities.Enums;
using MainServer.Exceptions;
using MainServer.Helpers;
using MainServer.Mappings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MainServer.Services;

public class AuthService(
    AppDbContext context,
    JwtTokenGenerator tokenGenerator,
    ILogger<AuthService> logger)
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await context.Users
            .AnyAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            Role = UserRole.Customer,
            CreatedAt = now,
            UpdatedAt = now
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User registered with email {Email}", user.Email);

        return new ApiResponse<UserResponse>(true, user.ToResponse());
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var (token, expiresAt) = tokenGenerator.GenerateToken(user);

        logger.LogInformation("User logged in with email {Email}", user.Email);

        return new ApiResponse<LoginResponse>(true, new LoginResponse(
            token,
            expiresAt,
            user.Id,
            user.Email,
            user.Role.ToString()));
    }
}
