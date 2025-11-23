namespace Shared.Infrastructure;

/// <summary>
/// Interface for publishing messages to RabbitMQ
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to RabbitMQ
    /// </summary>
    /// <param name="exchange">The exchange name</param>
    /// <param name="routingKey">The routing key</param>
    /// <param name="message">The message object to serialize and publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync(string exchange, string routingKey, object message, CancellationToken cancellationToken = default);
}

