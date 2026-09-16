namespace NotificationService.Providers;

public interface INotificationProvider
{
    Task SendAsync(string title, string message, CancellationToken cancellationToken = default);
}
