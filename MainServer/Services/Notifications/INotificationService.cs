using MainServer.DTOs.Common;
using MainServer.DTOs.Notifications;

namespace MainServer.Services.Notifications;

public interface INotificationService
{
    Task<ApiResponse<PaginationResponse<NotificationResponse>>> GetAsync(
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default);
}
