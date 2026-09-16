using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PaymentService.Messaging;

public class RabbitMqEventPublisher(IOptions<RabbitMqSettings> options)
    : IEventPublisher,
        IAsyncDisposable
{
    private readonly RabbitMqSettings settings = options.Value;
    private readonly SemaphoreSlim gate = new(1, 1);
    private IConnection? connection;
    private IChannel? channel;

    public async Task PublishAsync(
        string routingKey,
        string payload,
        CancellationToken cancellationToken = default
    )
    {
        var activeChannel = await GetChannelAsync(cancellationToken);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
        };

        await activeChannel.BasicPublishAsync(
            exchange: settings.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(payload),
            cancellationToken: cancellationToken
        );
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (channel is { IsOpen: true })
        {
            return channel;
        }

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (channel is { IsOpen: true })
            {
                return channel;
            }

            if (connection is null || !connection.IsOpen)
            {
                var factory = new ConnectionFactory
                {
                    HostName = settings.HostName,
                    Port = settings.Port,
                    UserName = settings.UserName,
                    Password = settings.Password,
                    VirtualHost = settings.VirtualHost,
                };

                connection = await factory.CreateConnectionAsync(cancellationToken);
            }

            channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                exchange: settings.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            return channel;
        }
        finally
        {
            gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (channel is not null)
        {
            await channel.DisposeAsync();
        }

        if (connection is not null)
        {
            await connection.DisposeAsync();
        }

        gate.Dispose();
        GC.SuppressFinalize(this);
    }
}
