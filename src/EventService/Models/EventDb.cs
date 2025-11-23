using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace EventService.Models;

public class EventDb : DbContext
{
    public EventDb(DbContextOptions<EventDb> options) : base(options) { }
    
    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Difficulty).HasMaxLength(100);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Type);
        });

        modelBuilder.Entity<EventEntity>().HasData(
            new EventEntity 
            { 
                Id = 1, 
                Name = "Skeleton Fort", 
                Description = "A world event where skeleton crews defend a fort. Players must defeat waves of skeletons and a captain to claim the treasure.",
                Type = "World Event",
                Difficulty = "Medium",
                MinPlayers = 1,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 20,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 3, 20)
            },
            new EventEntity 
            { 
                Id = 2, 
                Name = "Ashen Winds", 
                Description = "A world event featuring an Ashen Lord skeleton boss. Players battle the Ashen Lord to claim Ashen Winds Skull and treasure.",
                Type = "World Event",
                Difficulty = "Hard",
                MinPlayers = 2,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 30,
                IsActive = true,
                IntroducedDate = new DateTime(2020, 6, 11)
            },
            new EventEntity 
            { 
                Id = 3, 
                Name = "Fort of the Damned", 
                Description = "A legendary fort event requiring ritual skulls and colored flames. Offers the highest value loot in the game.",
                Type = "World Event",
                Difficulty = "Legendary",
                MinPlayers = 3,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 45,
                IsActive = true,
                IntroducedDate = new DateTime(2019, 10, 16)
            },
            new EventEntity 
            { 
                Id = 4, 
                Name = "Kraken", 
                Description = "A random encounter with a massive sea monster. The Kraken can attack any ship sailing in open waters.",
                Type = "Encounter",
                Difficulty = "Hard",
                MinPlayers = 1,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 15,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 3, 20)
            },
            new EventEntity 
            { 
                Id = 5, 
                Name = "Megalodon", 
                Description = "A random encounter with a giant shark. Multiple variants exist, each with different difficulty levels.",
                Type = "Encounter",
                Difficulty = "Medium",
                MinPlayers = 1,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 10,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 5, 29)
            },
            new EventEntity 
            { 
                Id = 6, 
                Name = "Skeleton Fleet", 
                Description = "A world event where skeleton ships emerge from the water. Players must defeat multiple waves of skeleton ships.",
                Type = "World Event",
                Difficulty = "Hard",
                MinPlayers = 2,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 25,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 7, 31)
            },
            new EventEntity 
            { 
                Id = 7, 
                Name = "Ghost Fleet", 
                Description = "A world event featuring ghost ships led by Flameheart. Players battle spectral ships in the sky.",
                Type = "World Event",
                Difficulty = "Hard",
                MinPlayers = 2,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 30,
                IsActive = true,
                IntroducedDate = new DateTime(2020, 4, 22)
            },
            new EventEntity 
            { 
                Id = 8, 
                Name = "Megalodon Hunting Voyage", 
                Description = "A voyage from The Hunter's Call allowing crews to track and battle an enraged Megalodon. Available at rank 20.",
                Type = "Voyage",
                Difficulty = "Hard",
                MinPlayers = 1,
                MaxPlayers = 4,
                EstimatedDurationMinutes = 20,
                IsActive = true,
                IntroducedDate = new DateTime(2025, 4, 24)
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
