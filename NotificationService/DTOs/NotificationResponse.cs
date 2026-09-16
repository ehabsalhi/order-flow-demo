using NotificationService.Entities.Enums;

namespace NotificationService.DTOs;

public record NotificationResponse(
    int Id,
    int OrderId,
    int PaymentId,
    NotificationType Type,
    string Title,
    string Message,
    NotificationStatus Status,
    DateTime CreatedAt);
