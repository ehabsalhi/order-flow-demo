namespace MainServer.DTOs.Notifications;

public enum NotificationType
{
    PaymentSucceeded = 0,
    PaymentFailed = 1
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}

public record NotificationResponse(
    int Id,
    int OrderId,
    int PaymentId,
    NotificationType Type,
    string Title,
    string Message,
    NotificationStatus Status,
    DateTime CreatedAt);
