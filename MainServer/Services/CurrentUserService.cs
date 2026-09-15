using MainServer.Helpers;

namespace MainServer.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsAdmin => httpContextAccessor.HttpContext?.User.IsAdmin() == true;

    public int UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User
                ?? throw new InvalidOperationException("No authenticated user in the current context.");

            return user.GetUserId();
        }
    }
}
