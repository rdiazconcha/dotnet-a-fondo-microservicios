using Microsoft.EntityFrameworkCore;

namespace FabuRobotics.Catalog.Api;

public class CatalogDbContext (DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(product =>
        {
            product.HasKey(p => p.Sku);
            product.HasData(
                    new("FB-01", "Home assistant robot", "Service Robots", 11999m, 12),
                    new("FB-02", "Hospitality and customer service robot", "Service Robots", 12999m, 8),
                    new("FB-03", "All-terrain flagship robot", "Service Robots", 10999m, 20),
                    new("FB-BAT-16", "16-hour battery pack for FB-03", "Components", 499m, 150),
                    new("FB-DOCK", "Self-docking charging station", "Accessories", 299m, 75));
        });
    }
}