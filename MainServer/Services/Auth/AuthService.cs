using AutoMapper;
using MainServer.DTOs.Auth;
using MainServer.DTOs.Common;
using MainServer.Entities;
using MainServer.Entities.Enums;
using MainServer.Exceptions;
using MainServer.Helpers;
using MainServer.Repositories.Users;
using Microsoft.AspNetCore.Identity;

namespace MainServer.Services.Auth;

public class AuthService(
    IUserRepository userRepository,
    JwtTokenGenerator tokenGenerator,
    IMapper mapper,
    ILogger<AuthService> logger
) : IAuthService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public async Task<ApiResponse<UserResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var isEmailExists = await userRepository.EmailExistsAsync(
            request.NormalizedEmail,
            cancellationToken
        );

        if (isEmailExists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.NormalizedEmail,
            Role = UserRole.Customer,
            CreatedAt = now,
            UpdatedAt = now,
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        user = await userRepository.AddAsync(user, cancellationToken);

        logger.LogInformation("User registered with email {Email}", user.Email);

        return new ApiResponse<UserResponse>(true, mapper.Map<UserResponse>(user));
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var user =
            await userRepository.GetByEmailAsync(request.NormalizedEmail, cancellationToken)
            ?? throw new UnauthorizedAppException("Invalid email or password.");

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var (token, _) = tokenGenerator.GenerateToken(user);

        logger.LogInformation("User logged in with email {Email}", user.Email);

        return new ApiResponse<LoginResponse>(
            true,
            new LoginResponse(token, user.Id, user.Email, user.Role.ToString())
        );
    }
}
