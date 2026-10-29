using Microsoft.EntityFrameworkCore;

namespace FabuRobotics.Orders.Api;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/orders");

        group.MapPost("/", PlaceOrder);
        group.MapGet("/{orderId:guid}", GetOrder);
        group.MapGet("/", GetOrdersByCustomer);

        return endpoints;
    }

    private static async Task<IResult> PlaceOrder(PlaceOrderRequest request, OrdersDbContext db, CancellationToken cancellationToken)
    {
        if (request.Lines is not { Count: > 0 })
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["lines"] = ["At least one line is required."]
            });
        }

        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = request.CustomerId,
            PlacedAt = DateTime.UtcNow,
            Lines = request.Lines.Select(line => new OrderLine { Sku = line.Sku, Quantity = line.Quantity }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/orders/{order.Id}", order);
    }

    private static async Task<IResult> GetOrder(Guid orderId, OrdersDbContext db, CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(o => o.Lines).SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        return order is null ? Results.NotFound() : Results.Ok(order);
    }

    private static async Task<IResult> GetOrdersByCustomer(string customerId, OrdersDbContext db, CancellationToken cancellationToken)
    {
        var orders = await db.Orders.Include(o => o.Lines).Where(o => o.CustomerId == customerId).ToListAsync(cancellationToken);

        return Results.Ok(orders);
    }
}
