var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

var products = new List<Product>
{
    new("FB-01", "Home assistant robot", "Service Robots", 11999m, 12),
    new("FB-02", "Hospitality and customer service robot", "Service Robots", 12999m, 8),
    new("FB-03", "All-terrain flagship robot", "Service Robots", 10999m, 20),
    new("FB-BAT-16", "16-hour battery pack for FB-03", "Components", 499m, 150),
    new("FB-DOCK", "Self-docking charging station", "Accessories", 299m, 75)
};

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/catalog/products", (string? category) =>
{
    return category is null ? products : products.Where(p => p.Category == category);
});

app.MapGet("/api/catalog/products/{sku}", (string sku) =>
{
    return products.SingleOrDefault(p => p.Sku == sku) is { } product ? 
        Results.Ok(product) : Results.NotFound();
});
    
app.Run();

public record Product(string Sku, string Name, string Category, decimal Price, int Stock);