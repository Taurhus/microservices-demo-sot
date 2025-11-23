using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Infrastructure;

namespace AchievementService.Models;

public class AchievementDb : DbContext
{
    public AchievementDb(DbContextOptions<AchievementDb> options) : base(options) { }
    
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
            entity.Property(a => a.Description).HasMaxLength(500);
            entity.Property(a => a.Category).HasMaxLength(100);
            entity.Property(a => a.Rarity).HasMaxLength(50);
            entity.Property(a => a.Notes).HasMaxLength(500);
            entity.HasIndex(a => a.PlayerId);
            entity.HasIndex(a => a.Name);
            entity.HasIndex(a => a.Category);
        });

        modelBuilder.Entity<Achievement>().HasData(
            new Achievement 
            { 
                Id = 1, 
                PlayerId = 1,
                Name = "Pirate Legend",
                Description = "Reach level 50 in three trading companies",
                Category = "Prestige",
                Rarity = "Legendary",
                UnlockedDate = new DateTime(2018, 9, 19),
                Progress = 100,
                RequiredProgress = 100,
                Notes = "Ultimate achievement"
            },
            new Achievement 
            { 
                Id = 2, 
                PlayerId = 1,
                Name = "Master Gold Hoarder",
                Description = "Reach level 75 with Gold Hoarders",
                Category = "Trading",
                Rarity = "Epic",
                UnlockedDate = new DateTime(2018, 8, 15),
                Progress = 100,
                RequiredProgress = 100,
                Notes = "Max reputation achieved"
            },
            new Achievement 
            { 
                Id = 3, 
                PlayerId = 2,
                Name = "Kraken Slayer",
                Description = "Defeat the Kraken 10 times",
                Category = "Combat",
                Rarity = "Rare",
                UnlockedDate = new DateTime(2024, 12, 10, 10, 30, 0, DateTimeKind.Utc),
                Progress = 100,
                RequiredProgress = 100,
                Notes = "Combat achievement"
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

