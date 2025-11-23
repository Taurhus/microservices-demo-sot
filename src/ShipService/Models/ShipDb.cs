using Microsoft.EntityFrameworkCore;
using ShipService.Models;
using Shared.Infrastructure;

namespace ShipService;

public class ShipDb : DbContext
{
    public ShipDb(DbContextOptions<ShipDb> options) : base(options) { }
    
    public DbSet<Ship> Ships => Set<Ship>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Type).HasMaxLength(50);
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.HasIndex(s => s.Name);
            entity.HasIndex(s => s.Type);
        });

        modelBuilder.Entity<Ship>().HasData(
            new Ship 
            { 
                Id = 1, 
                Name = "Sloop", 
                Type = "Sloop",
                Description = "A small, fast ship perfect for solo players or two-person crews. Most maneuverable ship in the game.",
                MaxCrewSize = 2,
                CannonCount = 2,
                MastCount = 1,
                IsActive = true
            },
            new Ship 
            { 
                Id = 2, 
                Name = "Brigantine", 
                Type = "Brigantine",
                Description = "A medium-sized ship balanced for speed and firepower. Ideal for three-person crews.",
                MaxCrewSize = 3,
                CannonCount = 4,
                MastCount = 2,
                IsActive = true
            },
            new Ship 
            { 
                Id = 3, 
                Name = "Galleon", 
                Type = "Galleon",
                Description = "The largest and most powerful ship. Requires a full crew of four to operate effectively.",
                MaxCrewSize = 4,
                CannonCount = 8,
                MastCount = 3,
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
