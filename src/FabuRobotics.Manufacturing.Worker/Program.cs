using FabuRobotics.Manufacturing.Worker;
using FabuRobotics.Shared;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ManufacturingDb")!;

builder.Services.AddDbContext<ManufacturingDbContext>(options => options.UseSqlite(connectionString));

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
builder.Services.AddHostedService<OrderPlacedConsumer>();

builder.AddDefaultHealthChecks()
       .AddSqlite(connectionString, name: "manufacturing-db")
       .AddRabbitMQ(name: "rabbitmq");

builder.AddObservability();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ManufacturingDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapDefaultHealthEndpoints();

app.Run();