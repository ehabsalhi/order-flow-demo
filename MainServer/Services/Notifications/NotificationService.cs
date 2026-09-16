using MainServer.DTOs.Common;
using MainServer.DTOs.Notifications;
using MainServer.Integrations.Notifications;

namespace MainServer.Services.Notifications;

public class NotificationService(INotificationHttpGateway notificationHttpGateway)
    : INotificationService
{
    public async Task<ApiResponse<PaginationResponse<NotificationResponse>>> GetAsync(
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var paged = await notificationHttpGateway.GetAsync(request, cancellationToken);
        return new ApiResponse<PaginationResponse<NotificationResponse>>(true, paged);
    }
}
