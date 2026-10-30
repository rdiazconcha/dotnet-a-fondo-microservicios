using System.Text.Json;
using FabuRobotics.Contracts;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FabuRobotics.Manufacturing.Worker;

public class OrderPlacedConsumer(IConnection connection,
                                 IServiceScopeFactory scopeFactory,
                                 ILogger<OrderPlacedConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                await HandleAsync(eventArgs, stoppingToken);
                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process message {MessageId}", eventArgs.BasicProperties.MessageId);
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: !eventArgs.Redelivered, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(MessagingTopology.ManufacturingOrderPlacedQueue, autoAck: false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var orderPlaced = JsonSerializer.Deserialize<OrderPlaced>(eventArgs.Body.Span, JsonSerializerOptions.Web)!;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ManufacturingDbContext>();

        if (await db.BuildJobs.AnyAsync(job => job.OrderId == orderPlaced.OrderId, cancellationToken))
        {
            logger.LogInformation("Order {OrderId} was already processed", orderPlaced.OrderId);
            return;
        }

        foreach (var line in orderPlaced.Lines)
        {
            for (var unit = 0; unit < line.Quantity; unit++)
            {
                db.BuildJobs.Add(new BuildJob
                {
                    Id = Guid.CreateVersion7(),
                    OrderId = orderPlaced.OrderId,
                    Sku = line.Sku,
                    SerialNumber = $"{line.Sku}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Build jobs created for order {OrderId}", orderPlaced.OrderId);
    }
}