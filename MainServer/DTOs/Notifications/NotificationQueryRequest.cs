using MainServer.DTOs.Common;

namespace MainServer.DTOs.Notifications;

public class NotificationQueryRequest : PaginationRequest
{
    public int? OrderId { get; set; }
    public NotificationStatus? Status { get; set; }
}
