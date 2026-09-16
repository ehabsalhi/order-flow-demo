namespace NotificationService.Messaging;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "payments";
    public string Queue { get; set; } = "notification-service.payment-events";
    public int ReconnectDelaySeconds { get; set; } = 5;
}
