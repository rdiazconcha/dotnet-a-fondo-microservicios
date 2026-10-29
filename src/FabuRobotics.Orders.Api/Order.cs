public class Order
{
    public Guid Id { get; set; }
    public required string CustomerId { get; set; }
    public string Status { get; set; } = "Placed";
    public DateTime PlacedAt { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
}