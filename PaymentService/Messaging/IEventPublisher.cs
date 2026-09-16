namespace PaymentService.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(
        string routingKey,
        string payload,
        CancellationToken cancellationToken = default
    );
}
