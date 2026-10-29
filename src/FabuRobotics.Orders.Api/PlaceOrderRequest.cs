namespace FabuRobotics.Orders.Api;

public record PlaceOrderRequest(string CustomerId, List<PlaceOrderLine> Lines);

public record PlaceOrderLine(string Sku, int Quantity);
