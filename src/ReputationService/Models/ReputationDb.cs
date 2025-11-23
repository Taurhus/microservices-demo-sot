using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Infrastructure;

namespace ReputationService.Models;

public class ReputationDb : DbContext
{
    public ReputationDb(DbContextOptions<ReputationDb> options) : base(options) { }
    
    public DbSet<Reputation> Reputations => Set<Reputation>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reputation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.FactionName).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Notes).HasMaxLength(500);
            entity.HasIndex(r => r.PlayerId);
            entity.HasIndex(r => r.FactionName);
            entity.HasIndex(r => new { r.PlayerId, r.FactionName }).IsUnique();
        });

        // Configure OutboxEvent entity for transactional event publishing
        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Exchange).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RoutingKey).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MessageBody).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<Reputation>().HasData(
            new Reputation 
            { 
                Id = 1, 
                PlayerId = 1,
                FactionName = "Gold Hoarders",
                Level = 75,
                TotalReputation = 500000,
                LastUpdated = new DateTime(2024, 12, 15, 10, 30, 0, DateTimeKind.Utc),
                Notes = "Max reputation level"
            },
            new Reputation 
            { 
                Id = 2, 
                PlayerId = 1,
                FactionName = "Order of Souls",
                Level = 50,
                TotalReputation = 250000,
                LastUpdated = new DateTime(2024, 12, 14, 10, 30, 0, DateTimeKind.Utc),
                Notes = "Halfway to max"
            },
            new Reputation 
            { 
                Id = 3, 
                PlayerId = 2,
                FactionName = "Merchant Alliance",
                Level = 30,
                TotalReputation = 100000,
                LastUpdated = new DateTime(2024, 12, 15, 10, 30, 0, DateTimeKind.Utc),
                Notes = "Growing reputation"
            }
        );
    }
}

