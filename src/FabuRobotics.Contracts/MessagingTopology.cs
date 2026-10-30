namespace FabuRobotics.Contracts;

public static class MessagingTopology
{
    public const string EventsExchange = "faburobotics.events";
    public const string DeadLetterExchange = "faburobotics.dead-letter";
    public const string OrderPlacedRoutingKey = "orders.order-placed";
    public const string ManufacturingOrderPlacedQueue = "manufacturing.order-placed";
}
