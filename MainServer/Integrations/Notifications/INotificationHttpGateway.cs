using MainServer.DTOs.Common;
using MainServer.DTOs.Notifications;

namespace MainServer.Integrations.Notifications;

public interface INotificationHttpGateway
{
    Task<PaginationResponse<NotificationResponse>> GetAsync(
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default);
}
