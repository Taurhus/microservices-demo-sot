using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Shared.Infrastructure;

/// <summary>
/// Transactional message publisher using outbox pattern
/// Saves events to database in the same transaction as business data
/// </summary>
public class TransactionalMessagePublisher : ITransactionalMessagePublisher
{
    private readonly ILogger<TransactionalMessagePublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public TransactionalMessagePublisher(ILogger<TransactionalMessagePublisher> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task SaveEventToOutboxAsync<TDbContext>(TDbContext dbContext, string exchange, string routingKey, object message, CancellationToken cancellationToken = default) where TDbContext : DbContext
    {
        try
        {
            // Serialize message
            var messageBody = JsonSerializer.Serialize(message, _jsonOptions);

            // Create outbox event
            var outboxEvent = new OutboxEvent
            {
                Exchange = exchange,
                RoutingKey = routingKey,
                MessageBody = messageBody,
                CreatedAt = DateTime.UtcNow
            };

            // Add to database context (will be saved in the same transaction)
            dbContext.Set<OutboxEvent>().Add(outboxEvent);

            // Note: We don't call SaveChangesAsync here - it should be called
            // by the caller after this method, ensuring both the business entity
            // and the outbox event are saved in the same transaction

            _logger.LogDebug("Event saved to outbox: {Exchange}/{RoutingKey}", exchange, routingKey);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save event to outbox: {Exchange}/{RoutingKey}", exchange, routingKey);
            throw; // Re-throw to ensure transaction rollback
        }
    }
}

