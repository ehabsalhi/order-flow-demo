namespace MainServer.DTOs.Auth;

public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    int UserId,
    string Email,
    string Role);
