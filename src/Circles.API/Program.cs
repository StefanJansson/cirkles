using Circles.Application.Authorization;
using Circles.Application.Services;
using Circles.Domain.Interfaces;
using Circles.Infrastructure.Persistence;
using Circles.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---- Services -------------------------------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Serialize enums as their string names for readable, stable API output.
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core / PostgreSQL. Connection string comes from configuration
// (appsettings.json) and can be overridden via the ConnectionStrings__Circles
// environment variable.
var connectionString = builder.Configuration.GetConnectionString("Circles")
    ?? "Host=localhost;Port=5432;Database=circles;Username=postgres;Password=postgres";

builder.Services.AddDbContext<CirclesDbContext>(options =>
    options.UseNpgsql(connectionString));

// Application services.
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<CirclesQueryService>();

var app = builder.Build();

// ---- Migrate + seed on startup -------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CirclesDbContext>();
    await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

// ---- HTTP pipeline --------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Health check endpoint.
app.MapGet("/health", () => Results.Ok(new { status = "healthy", time = DateTime.UtcNow }));

app.Run();

// Exposed so integration tests / tooling can reference the entry point.
public partial class Program { }
