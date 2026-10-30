using FabuRobotics.Manufacturing.Worker;
using FabuRobotics.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ManufacturingDb")!;

builder.Services.AddDbContext<ManufacturingDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.AddDefaultHealthChecks()
       .AddSqlite(connectionString, name: "manufacturing-db");

builder.AddObservability();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ManufacturingDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapDefaultHealthEndpoints();

app.Run();