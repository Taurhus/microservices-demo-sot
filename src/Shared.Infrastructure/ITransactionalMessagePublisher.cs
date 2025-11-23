using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Infrastructure;

/// <summary>
/// Interface for transactional message publishing using outbox pattern
/// </summary>
public interface ITransactionalMessagePublisher
{
    /// <summary>
    /// Saves an event to the outbox table (must be called within a database transaction)
    /// </summary>
    Task SaveEventToOutboxAsync<TDbContext>(TDbContext dbContext, string exchange, string routingKey, object message, CancellationToken cancellationToken = default) where TDbContext : DbContext;
}

