using FabuRobotics.Orders.Api;
using FabuRobotics.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("OrdersDb")!;

builder.Services.AddDbContext<OrdersDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog:BaseUrl"]!);
}).AddStandardResilienceHandler();

builder.AddDefaultHealthChecks()
    .AddSqlite(connectionString, name: "orders-db");

builder.AddObservability();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapOrderEndpoints();
app.MapDefaultHealthEndpoints();

app.Run();