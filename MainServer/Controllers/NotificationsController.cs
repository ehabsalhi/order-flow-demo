using MainServer.DTOs.Common;
using MainServer.DTOs.Notifications;
using MainServer.Entities.Enums;
using MainServer.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainServer.Controllers;

[Route("api/notifications")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class NotificationsController(INotificationService notificationService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginationResponse<NotificationResponse>>>(
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] NotificationQueryRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await notificationService.GetAsync(request, cancellationToken);
        return Ok(result);
    }
}
