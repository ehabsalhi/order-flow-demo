namespace NotificationService.Providers;

public class MockNotificationProvider(ILogger<MockNotificationProvider> logger)
    : INotificationProvider
{
    public Task SendAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Mock notification sent: {Title} — {Message}", title, message);
        return Task.CompletedTask;
    }
}
