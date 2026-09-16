using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.DTOs.Common;
using NotificationService.Entities;
using NotificationService.Entities.Enums;
using NotificationService.Helpers;
using NotificationService.Messaging;
using NotificationService.Providers;

namespace NotificationService.Services;

public class NotificationService(
    NotificationDbContext context,
    INotificationProvider notificationProvider,
    ILogger<NotificationService> logger
) : INotificationService
{
    public async Task RecordPaymentEventAsync(
        PaymentStatusChangedEvent statusEvent,
        CancellationToken cancellationToken = default
    )
    {
        var type = MapType(statusEvent.Status);
        if (type is not { } notificationType)
        {
            return;
        }

        var alreadyRecorded = await context.Notifications.AnyAsync(
            n => n.PaymentId == statusEvent.PaymentId && n.Type == notificationType,
            cancellationToken
        );

        if (alreadyRecorded)
        {
            return;
        }

        var (title, message) = BuildContent(notificationType, statusEvent);
        var notification = new Notification
        {
            OrderId = statusEvent.OrderId,
            PaymentId = statusEvent.PaymentId,
            Type = notificationType,
            Title = title,
            Message = message,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(cancellationToken);

        await notificationProvider.SendAsync(title, message, cancellationToken);

        notification.Status = NotificationStatus.Sent;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Notification {NotificationId} recorded for order {OrderId} ({Type}).",
            notification.Id,
            notification.OrderId,
            notification.Type
        );
    }

    public async Task<ApiResponse<PaginationResponse<NotificationResponse>>> GetAsync(
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = context.Notifications.AsNoTracking().AsQueryable();

        if (request.OrderId.HasValue)
        {
            query = query.Where(n => n.OrderId == request.OrderId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(n => n.Status == request.Status.Value);
        }

        var projected = query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse(
                n.Id,
                n.OrderId,
                n.PaymentId,
                n.Type,
                n.Title,
                n.Message,
                n.Status,
                n.CreatedAt
            ));

        var paged = await projected.ToPagedResultAsync(request, cancellationToken);
        return new ApiResponse<PaginationResponse<NotificationResponse>>(true, paged);
    }

    private static NotificationType? MapType(string paymentStatus) =>
        paymentStatus switch
        {
            "Succeeded" => NotificationType.PaymentSucceeded,
            "Failed" => NotificationType.PaymentFailed,
            _ => null,
        };

    private static (string Title, string Message) BuildContent(
        NotificationType type,
        PaymentStatusChangedEvent statusEvent
    )
    {
        return type switch
        {
            NotificationType.PaymentSucceeded => (
                "Payment succeeded",
                $"Payment {statusEvent.PaymentId} for order {statusEvent.OrderId} succeeded ({statusEvent.Amount} {statusEvent.Currency})."
            ),
            NotificationType.PaymentFailed => (
                "Payment failed",
                $"Payment {statusEvent.PaymentId} for order {statusEvent.OrderId} failed ({statusEvent.Amount} {statusEvent.Currency})."
            ),
            _ => (
                "Payment update",
                $"Payment {statusEvent.PaymentId} for order {statusEvent.OrderId} was updated."
            ),
        };
    }
}
