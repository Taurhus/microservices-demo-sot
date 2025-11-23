using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure;

/// <summary>
/// Extension methods for service collection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RabbitMQ message publishing to the service collection
    /// </summary>
    public static IServiceCollection AddRabbitMQMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IMessagePublisher, RabbitMQMessagePublisher>();
        return services;
    }

    /// <summary>
    /// Adds transactional message publishing using outbox pattern
    /// </summary>
    public static IServiceCollection AddTransactionalMessaging<TContext>(this IServiceCollection services) 
        where TContext : DbContext
    {
        // Register transactional publisher as scoped service
        services.AddScoped<ITransactionalMessagePublisher, TransactionalMessagePublisher>();
        
        // Note: OutboxProcessor can be added separately if needed for background processing
        // For now, events are saved to the outbox table atomically with business data
        // A separate background service can process the outbox table later
        
        return services;
    }
}

