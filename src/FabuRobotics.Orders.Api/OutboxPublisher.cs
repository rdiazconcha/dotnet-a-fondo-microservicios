using FabuRobotics.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FabuRobotics.Orders.Api;

public class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    RabbitMqEventPublisher publisher,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingMessagesAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Failed to publish outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        var pending = await db.OutboxMessages
            .Where(message => message.PublishedAt == null)
            .OrderBy(message => message.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            await publisher.PublishAsync(
                MessagingTopology.OrderPlacedRoutingKey,
                message.Type,
                message.Id.ToString(),
                message.Payload,
                cancellationToken);

            message.PublishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}