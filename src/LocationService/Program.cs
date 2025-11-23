using LocationService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddDbContext<LocationDb>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=azuresql;Database=LocationDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Location Service API", Version = "v1" });
});

// Add RabbitMQ messaging
builder.Services.AddRabbitMQMessaging();

// Add transactional messaging (outbox pattern)
builder.Services.AddTransactionalMessaging<LocationDb>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<LocationDb>();

// Add response compression
builder.Services.AddResponseCompression();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure pipeline
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Location Service API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// Initialize database asynchronously
var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    await DatabaseInitializer.InitializeDatabaseAsync<LocationDb>(
        app.Services, 
        "LocationDb", 
        logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize database");
    throw;
}

logger.LogInformation("LocationService started successfully");
app.Run();
