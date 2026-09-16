using NotificationService.Entities.Enums;

namespace NotificationService.Entities;

public class Notification
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int PaymentId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public DateTime CreatedAt { get; set; }
}
