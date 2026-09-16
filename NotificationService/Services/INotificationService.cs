using NotificationService.DTOs;
using NotificationService.DTOs.Common;
using NotificationService.Messaging;

namespace NotificationService.Services;

public interface INotificationService
{
    Task RecordPaymentEventAsync(
        PaymentStatusChangedEvent statusEvent,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PaginationResponse<NotificationResponse>>> GetAsync(
        NotificationQueryRequest request,
        CancellationToken cancellationToken = default);
}
