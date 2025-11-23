using AchievementService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddDbContext<AchievementDb>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=azuresql;Database=AchievementDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Achievement Service API", Version = "v1" });
});

// Add RabbitMQ messaging
builder.Services.AddRabbitMQMessaging();

// Add transactional messaging (outbox pattern)
builder.Services.AddTransactionalMessaging<AchievementDb>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AchievementDb>();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Achievement Service API v1");
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
    await DatabaseInitializer.InitializeDatabaseAsync<AchievementDb>(
        app.Services, 
        "AchievementDb", 
        logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize database");
    throw;
}

logger.LogInformation("AchievementService started successfully");
app.Run();

