namespace MainServer.DTOs.Auth;

public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    DateTime CreatedAt);
