namespace FabuRobotics.Contracts;

public record OrderPlaced(Guid OrderId, string CustomerId, IReadOnlyList<OrderPlacedLine> Lines, DateTime PlacedAt);

public record OrderPlacedLine(string Sku, int Quantity, decimal UnitPrice);
