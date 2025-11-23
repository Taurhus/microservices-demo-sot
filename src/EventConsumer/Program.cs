using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Event Consumer Service starting...");

// RabbitMQ connection
var factory = new ConnectionFactory
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq",
    Port = int.TryParse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"), out var port) ? port : 5672,
    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
};

IConnection? connection = null;
IModel? channel = null;

try
{
    connection = factory.CreateConnection();
    channel = connection.CreateModel();
    
    // Declare exchange
    channel.ExchangeDeclare(exchange: "seaofthieves.events", type: ExchangeType.Topic, durable: true);
    
    // Declare queue
    var queueName = channel.QueueDeclare("event-consumer-queue", durable: true, exclusive: false, autoDelete: false).QueueName;
    
    // Bind to all routing keys
    channel.QueueBind(queue: queueName, exchange: "seaofthieves.events", routingKey: "#");
    
    logger.LogInformation("Connected to RabbitMQ. Queue: {QueueName}", queueName);
    
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var routingKey = ea.RoutingKey;
        
        try
        {
            // Parse message
            var jsonDoc = JsonDocument.Parse(message);
            var eventType = routingKey.Split('.').FirstOrDefault() ?? "unknown";
            var action = routingKey.Split('.').LastOrDefault() ?? "unknown";
            
            logger.LogInformation("Received event: {RoutingKey} | EventType: {EventType} | Action: {Action}", 
                routingKey, eventType, action);
            
            // Log event details
            if (jsonDoc.RootElement.TryGetProperty("messageId", out var messageId))
            {
                logger.LogInformation("Message ID: {MessageId}", messageId.GetString());
            }
            
            // Process different event types
            switch (eventType.ToLower())
            {
                case "player":
                    logger.LogInformation("Player event processed: {Action} - {Message}", action, message);
                    break;
                case "ship":
                    logger.LogInformation("Ship event processed: {Action} - {Message}", action, message);
                    break;
                case "achievement":
                    logger.LogInformation("Achievement event processed: {Action} - {Message}", action, message);
                    break;
                case "reputation":
                    logger.LogInformation("Reputation event processed: {Action} - {Message}", action, message);
                    break;
                default:
                    logger.LogInformation("Generic event processed: {RoutingKey} - {Message}", routingKey, message);
                    break;
            }
            
            // Acknowledge message
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message: {RoutingKey}", routingKey);
            // Reject and requeue
            channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
        }
    };
    
    channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    
    logger.LogInformation("Event Consumer Service started. Waiting for events...");
    
    await host.RunAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to start Event Consumer Service");
    throw;
}
finally
{
    channel?.Close();
    channel?.Dispose();
    connection?.Close();
    connection?.Dispose();
}
