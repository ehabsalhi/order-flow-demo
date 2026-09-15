using System.ComponentModel.DataAnnotations;

namespace MainServer.DTOs.Auth;

public static class EmailNormalization
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var normalized = Normalize(email);
        return normalized.Length <= 256 && EmailValidator.IsValid(normalized);
    }
}
