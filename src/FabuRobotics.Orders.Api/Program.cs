using FabuRobotics.Orders.Api;
using FabuRobotics.Shared;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("OrdersDb")!;

builder.Services.AddDbContext<OrdersDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Catalog:BaseUrl"]!);
}).AddStandardResilienceHandler();

var rabbitMq = builder.Configuration.GetSection("RabbitMq");
var connectionFactory = new ConnectionFactory
{
    HostName = rabbitMq["Host"]!,
    Port = rabbitMq.GetValue("Port", 5672),
    UserName = rabbitMq["Username"]!,
    Password = rabbitMq["Password"]!
};

await using var connection = await connectionFactory.CreateConnectionAsync();
await connection.DeclareTopologyAsync();

builder.Services.AddSingleton(connection);
builder.Services.AddSingleton<RabbitMqEventPublisher>();

builder.AddDefaultHealthChecks()
    .AddSqlite(connectionString, name: "orders-db")
    .AddRabbitMQ(name: "rabbitmq");

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