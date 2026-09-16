using NotificationService.DTOs.Common;
using NotificationService.Entities.Enums;

namespace NotificationService.DTOs;

public class NotificationQueryRequest : PaginationRequest
{
    public int? OrderId { get; set; }
    public NotificationStatus? Status { get; set; }
}
