using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Infrastructure;

namespace EmissaryService.Models;

public class EmissaryDb : DbContext
{
    public EmissaryDb(DbContextOptions<EmissaryDb> options) : base(options) { }
    
    public DbSet<Emissary> Emissaries => Set<Emissary>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Emissary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FactionName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.HasIndex(e => e.PlayerId);
            entity.HasIndex(e => e.FactionName);
            entity.HasIndex(e => new { e.PlayerId, e.FactionName });
        });

        modelBuilder.Entity<Emissary>().HasData(
            new Emissary 
            { 
                Id = 1, 
                PlayerId = 1,
                FactionName = "Gold Hoarders",
                Level = 5,
                IsActive = true,
                Value = 15000,
                RaisedDate = new DateTime(2024, 12, 15, 8, 30, 0, DateTimeKind.Utc),
                Notes = "Max level emissary flag"
            },
            new Emissary 
            { 
                Id = 2, 
                PlayerId = 2,
                FactionName = "Order of Souls",
                Level = 3,
                IsActive = false,
                Value = 5000,
                RaisedDate = new DateTime(2024, 12, 14, 10, 30, 0, DateTimeKind.Utc),
                LoweredDate = new DateTime(2024, 12, 14, 22, 30, 0, DateTimeKind.Utc),
                Notes = "Recently lowered"
            },
            new Emissary 
            { 
                Id = 3, 
                PlayerId = 3,
                FactionName = "Reaper's Bones",
                Level = 5,
                IsActive = true,
                Value = 20000,
                RaisedDate = new DateTime(2024, 12, 15, 9, 30, 0, DateTimeKind.Utc),
                Notes = "Reaper emissary at max level"
            }
        );

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
    }
}

