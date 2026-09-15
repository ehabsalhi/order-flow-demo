using MainServer.DTOs.Auth;
using MainServer.DTOs.Common;
using MainServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<ApiResponse<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType<ApiResponse<LoginResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }
}
