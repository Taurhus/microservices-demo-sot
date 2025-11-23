using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace PlayerService.Models;

public class PlayerDb : DbContext
{
    public PlayerDb(DbContextOptions<PlayerDb> options) : base(options) { }
    
    public DbSet<Player> Players => Set<Player>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Gamertag).HasMaxLength(100);
            entity.Property(p => p.Platform).HasMaxLength(50);
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.Gamertag);
        });

        modelBuilder.Entity<Player>().HasData(
            new Player 
            { 
                Id = 1, 
                Name = "Captain Flameheart", 
                Gamertag = "Flameheart",
                Gold = 50000,
                Renown = 100,
                IsPirateLegend = true,
                Platform = "Xbox",
                CreatedDate = new DateTime(2018, 3, 20),
                LastLoginDate = new DateTime(2024, 12, 15, 10, 30, 0, DateTimeKind.Utc)
            },
            new Player 
            { 
                Id = 2, 
                Name = "Merrick", 
                Gamertag = "MerrickHunter",
                Gold = 25000,
                Renown = 75,
                IsPirateLegend = false,
                Platform = "Steam",
                CreatedDate = new DateTime(2019, 4, 30),
                LastLoginDate = new DateTime(2024, 12, 14, 10, 30, 0, DateTimeKind.Utc)
            },
            new Player 
            { 
                Id = 3, 
                Name = "Umbra", 
                Gamertag = "UmbraSeeker",
                Gold = 100000,
                Renown = 120,
                IsPirateLegend = true,
                Platform = "PlayStation",
                CreatedDate = new DateTime(2024, 4, 30),
                LastLoginDate = new DateTime(2024, 12, 15, 10, 30, 0, DateTimeKind.Utc)
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
