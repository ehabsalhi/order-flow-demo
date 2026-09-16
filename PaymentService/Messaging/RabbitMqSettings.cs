namespace PaymentService.Messaging;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "payments";
    public int PollingIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
    public int MaxRetryCount { get; set; } = 5;
}
