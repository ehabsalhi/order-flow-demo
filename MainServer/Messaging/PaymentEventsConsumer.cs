using System.Text;
using System.Text.Json;
using MainServer.Entities.Enums;
using MainServer.Repositories.Orders;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MainServer.Messaging;

public class PaymentEventsConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<PaymentRabbitMqSettings> options,
    ILogger<PaymentEventsConsumer> logger
) : BackgroundService
{
    private readonly PaymentRabbitMqSettings settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reconnectDelay = TimeSpan.FromSeconds(Math.Max(1, settings.ReconnectDelaySeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Payment events consumer disconnected; retrying in {Delay}s.",
                    reconnectDelay.TotalSeconds
                );
                await Task.Delay(reconnectDelay, stoppingToken);
            }
        }
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password,
            VirtualHost = settings.VirtualHost,
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken
        );

        await channel.ExchangeDeclareAsync(
            exchange: settings.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        await channel.QueueDeclareAsync(
            queue: settings.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        await channel.QueueBindAsync(
            queue: settings.Queue,
            exchange: settings.Exchange,
            routingKey: "payment.*",
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                await HandleMessageAsync(ea, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process payment event.");
                await channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    stoppingToken
                );
            }
        };

        await channel.BasicConsumeAsync(
            queue: settings.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        logger.LogInformation("Listening for payment events on queue {Queue}.", settings.Queue);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(
        BasicDeliverEventArgs ea,
        CancellationToken cancellationToken
    )
    {
        var json = Encoding.UTF8.GetString(ea.Body.Span);
        var statusEvent = JsonSerializer.Deserialize<PaymentStatusChangedEvent>(json);

        if (statusEvent is null)
        {
            return;
        }

        var status = MapStatus(statusEvent.Status);
        if (status is not { } orderStatus)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var updated = await orderRepository.SetStatusAsync(
            statusEvent.OrderId,
            orderStatus,
            cancellationToken
        );

        if (updated)
        {
            logger.LogInformation(
                "Order {OrderId} updated to {Status} from payment event.",
                statusEvent.OrderId,
                orderStatus
            );
        }
    }

    private static OrderStatus? MapStatus(string paymentStatus) =>
        paymentStatus switch
        {
            "Succeeded" => OrderStatus.Paid,
            "Failed" => OrderStatus.Cancelled,
            _ => null,
        };
}
