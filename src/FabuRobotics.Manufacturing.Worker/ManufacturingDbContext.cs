using Microsoft.EntityFrameworkCore;

namespace FabuRobotics.Manufacturing.Worker;

public class ManufacturingDbContext(DbContextOptions<ManufacturingDbContext> options) : DbContext(options)
{
    public DbSet<BuildJob> BuildJobs => Set<BuildJob>();
}