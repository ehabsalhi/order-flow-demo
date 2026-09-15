using MainServer.DTOs.Auth;
using MainServer.DTOs.Common;

namespace MainServer.Services.Auth;

public interface IAuthService
{
    Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
