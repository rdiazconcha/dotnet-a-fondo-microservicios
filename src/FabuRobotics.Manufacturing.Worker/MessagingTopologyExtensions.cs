using FabuRobotics.Contracts;
using RabbitMQ.Client;

namespace FabuRobotics.Manufacturing.Worker;

public static class MessagingTopologyExtensions
{
    public static async Task DeclareTopologyAsync(this IConnection connection)
    {
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(MessagingTopology.EventsExchange, ExchangeType.Topic, durable: true, autoDelete: false);
        await channel.ExchangeDeclareAsync(MessagingTopology.DeadLetterExchange, ExchangeType.Topic, durable: true, autoDelete: false);

        await channel.QueueDeclareAsync(
            MessagingTopology.ManufacturingOrderPlacedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = MessagingTopology.DeadLetterExchange });

        await channel.QueueBindAsync(
            MessagingTopology.ManufacturingOrderPlacedQueue,
            MessagingTopology.EventsExchange,
            MessagingTopology.OrderPlacedRoutingKey);

        await channel.QueueDeclareAsync(
            MessagingTopology.ManufacturingOrderPlacedDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            MessagingTopology.ManufacturingOrderPlacedDeadLetterQueue,
            MessagingTopology.DeadLetterExchange,
            MessagingTopology.OrderPlacedRoutingKey);
    }
}
