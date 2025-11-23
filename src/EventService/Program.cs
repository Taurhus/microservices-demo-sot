using EventService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddDbContext<EventDb>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=azuresql;Database=EventDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Event Service API", Version = "v1" });
});

// Add RabbitMQ messaging
builder.Services.AddRabbitMQMessaging();

// Add transactional messaging (outbox pattern)
builder.Services.AddTransactionalMessaging<EventDb>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EventDb>();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Service API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

// Initialize database asynchronously
var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    await DatabaseInitializer.InitializeDatabaseAsync<EventDb>(
        app.Services, 
        "EventDb", 
        logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize database");
    throw;
}

logger.LogInformation("EventService started successfully");
app.Run();




