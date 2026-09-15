namespace MainServer.DTOs.Auth;

public record LoginResponse(
    string Token,
    int UserId,
    string Email,
    string Role
);
