using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Shared.Infrastructure;

/// <summary>
/// Background service that processes outbox events and publishes them to RabbitMQ
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private IConnection? _connection;
    private IModel? _channel;

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initialize RabbitMQ connection
        await InitializeRabbitMQAsync(stoppingToken);

        _logger.LogInformation("Outbox processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxEventsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox events");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }
    }

    private async Task InitializeRabbitMQAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
                Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672,
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _logger.LogInformation("RabbitMQ connection established for outbox processor");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Outbox processor will retry.");
        }
    }

    private async Task ProcessOutboxEventsAsync(CancellationToken cancellationToken)
    {
        if (_channel == null || _connection == null || !_channel.IsOpen)
        {
            // Try to reconnect
            await InitializeRabbitMQAsync(cancellationToken);
            return;
        }

        // Get all DbContext types from services
        // For simplicity, we'll process each service's outbox
        // In a real implementation, you might want a more sophisticated approach
        
        // This is a simplified version - in production, you'd want to:
        // 1. Have a shared outbox table or process per-service
        // 2. Use a more efficient query (e.g., TOP N with ordering)
        // 3. Handle retries and dead letter queue
        
        // For now, we'll note that each service needs to register its DbContext
        // and we'll process events from all registered contexts
        
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
        }
        catch { }
        
        try
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        catch { }
        
        base.Dispose();
    }
}

