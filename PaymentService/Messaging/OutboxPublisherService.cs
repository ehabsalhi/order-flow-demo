using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentService.Data;

namespace PaymentService.Messaging;

public class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    IEventPublisher publisher,
    IOptions<RabbitMqSettings> options,
    ILogger<OutboxPublisherService> logger
) : BackgroundService
{
    private readonly RabbitMqSettings settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(Math.Max(1, settings.PollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Outbox publishing cycle failed.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task PublishPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        var messages = await context
            .OutboxMessages.Where(m =>
                m.PublishedAt == null && m.RetryCount < settings.MaxRetryCount
            )
            .OrderBy(m => m.CreatedAt)
            .Take(settings.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(message.Type, message.Payload, cancellationToken);
                message.PublishedAt = DateTime.UtcNow;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.RetryCount++;

                if (message.RetryCount >= settings.MaxRetryCount)
                {
                    logger.LogError(
                        exception,
                        "Giving up on outbox message {MessageId} after {Retry} attempts; it will no longer be retried.",
                        message.Id,
                        message.RetryCount
                    );
                }
                else
                {
                    logger.LogWarning(
                        exception,
                        "Failed to publish outbox message {MessageId} (attempt {Retry}).",
                        message.Id,
                        message.RetryCount
                    );
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
