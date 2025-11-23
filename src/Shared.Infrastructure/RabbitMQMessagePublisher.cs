using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Shared.Infrastructure;

/// <summary>
/// RabbitMQ message publisher implementation
/// </summary>
public class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ILogger<RabbitMQMessagePublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMQMessagePublisher(ILogger<RabbitMQMessagePublisher> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        IConnection? connection = null;
        IModel? channel = null;
        
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
                Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672,
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            
            _logger.LogInformation("RabbitMQ connection established");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to RabbitMQ. Message publishing will be disabled.");
            // Connection/channel will remain null, and PublishAsync will handle it gracefully
        }
        
        _connection = connection!;
        _channel = channel!;
    }

    public async Task PublishAsync(string exchange, string routingKey, object message, CancellationToken cancellationToken = default)
    {
        if (_channel == null || _connection == null || !_channel.IsOpen)
        {
            _logger.LogWarning("RabbitMQ channel not available. Message not published: {Exchange}/{RoutingKey}", exchange, routingKey);
            return;
        }

        try
        {
            // Ensure exchange exists
            _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);

            // Serialize message
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            // Publish message
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published message to {Exchange}/{RoutingKey}: {MessageId}", exchange, routingKey, properties.MessageId);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to {Exchange}/{RoutingKey}", exchange, routingKey);
            // Don't throw - allow the operation to continue even if messaging fails
        }
    }

    public void Dispose()
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
    }
}

