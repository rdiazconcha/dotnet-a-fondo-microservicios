using System.Text;
using FabuRobotics.Contracts;
using RabbitMQ.Client;

namespace FabuRobotics.Orders.Api;

public class RabbitMqEventPublisher(IConnection connection)
{
    public async Task PublishAsync(string routingKey, string messageType, string messageId, string payload, CancellationToken cancellationToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            MessageId = messageId,
            Persistent = true,
            Headers = new Dictionary<string, object?> { ["message-type"] = messageType }
        };

        await channel.BasicPublishAsync(
            MessagingTopology.EventsExchange,
            routingKey,
            mandatory: false,
            properties,
            Encoding.UTF8.GetBytes(payload),
            cancellationToken);
    }
}
