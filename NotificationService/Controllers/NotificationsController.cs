using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.DTOs.Common;
using NotificationService.Services;

namespace NotificationService.Controllers;

[Route("api/notifications")]
public class NotificationsController(INotificationService notificationService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<ApiResponse<PaginationResponse<NotificationResponse>>>(
        StatusCodes.Status200OK
    )]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] NotificationQueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await notificationService.GetAsync(request, cancellationToken);
        return Ok(result);
    }
}
