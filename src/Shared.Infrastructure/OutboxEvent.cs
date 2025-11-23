using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Infrastructure;

/// <summary>
/// Outbox event entity for transactional event publishing
/// </summary>
public class OutboxEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Exchange { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string RoutingKey { get; set; } = string.Empty;

    [Required]
    public string MessageBody { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    public int RetryCount { get; set; } = 0;

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    // Computed property - not mapped to database
    [NotMapped]
    public bool IsPublished => PublishedAt.HasValue;
}

