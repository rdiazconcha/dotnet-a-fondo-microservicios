using FabuRobotics.Catalog.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<CatalogDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("CatalogDb"));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/catalog/products", async (string? category, CatalogDbContext db) =>
{
    return await db.Products.Where(p => category == null || p.Category == category).ToListAsync();
});

app.MapGet("/api/catalog/products/{sku}", async (string sku, CatalogDbContext db) =>
{
    return await db.Products.FindAsync(sku) is { } product ? Results.Ok(product) : Results.NotFound();
});
    

app.Run();

public record Product(string Sku, string Name, string Category, decimal Price, int Stock);