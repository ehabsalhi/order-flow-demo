namespace MainServer.DTOs.Auth;

public record LoginRequest(string Email, string Password)
{
    public string NormalizedEmail => EmailNormalization.Normalize(Email);
}
