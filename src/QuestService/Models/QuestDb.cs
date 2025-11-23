using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace QuestService.Models;

public class QuestDb : DbContext
{
    public QuestDb(DbContextOptions<QuestDb> options) : base(options) { }
    
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quest>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Name).IsRequired().HasMaxLength(200);
            entity.Property(q => q.Description).HasMaxLength(500);
            entity.Property(q => q.FactionName).HasMaxLength(100);
            entity.Property(q => q.Type).HasMaxLength(50);
            entity.HasIndex(q => q.Name);
            entity.HasIndex(q => q.FactionName);
            entity.HasIndex(q => q.Type);
        });

        modelBuilder.Entity<Quest>().HasData(
            new Quest 
            { 
                Id = 1, 
                Name = "Treasure Hunt", 
                Description = "A Gold Hoarders voyage involving maps and riddles to find buried treasure.",
                FactionName = "Gold Hoarders",
                Type = "Voyage",
                RequiredReputationLevel = 0,
                EstimatedDurationMinutes = 20,
                GoldReward = 500,
                IsActive = true
            },
            new Quest 
            { 
                Id = 2, 
                Name = "Skeleton Bounty", 
                Description = "An Order of Souls voyage to defeat skeleton captains and collect their skulls.",
                FactionName = "Order of Souls",
                Type = "Voyage",
                RequiredReputationLevel = 0,
                EstimatedDurationMinutes = 25,
                GoldReward = 600,
                IsActive = true
            },
            new Quest 
            { 
                Id = 3, 
                Name = "Cargo Run", 
                Description = "A Merchant Alliance voyage to deliver cargo between outposts on time.",
                FactionName = "Merchant Alliance",
                Type = "Voyage",
                RequiredReputationLevel = 0,
                EstimatedDurationMinutes = 15,
                GoldReward = 400,
                IsActive = true
            },
            new Quest 
            { 
                Id = 4, 
                Name = "Megalodon Hunting Voyage", 
                Description = "A Hunter's Call voyage to track and battle an enraged Megalodon. Requires rank 20.",
                FactionName = "Hunter's Call",
                Type = "Voyage",
                RequiredReputationLevel = 20,
                EstimatedDurationMinutes = 20,
                GoldReward = 2000,
                IsActive = true
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
