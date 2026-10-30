namespace FabuRobotics.Manufacturing.Worker;

public class BuildJob
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public required string Sku { get; set; }
    public required string SerialNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}