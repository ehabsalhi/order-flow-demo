using MainServer.DTOs.Auth;
using MainServer.Entities;

namespace MainServer.Mappings;

public static class AuthMappings
{
    public static UserResponse ToResponse(this User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString(), user.CreatedAt);
}
