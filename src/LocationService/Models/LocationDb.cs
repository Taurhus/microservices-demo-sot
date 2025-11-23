using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace LocationService.Models;

public class LocationDb : DbContext
{
    public LocationDb(DbContextOptions<LocationDb> options) : base(options) { }
    
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Type).HasMaxLength(50);
            entity.Property(l => l.Region).HasMaxLength(100);
            entity.HasIndex(l => l.Name);
            entity.HasIndex(l => l.Type);
            entity.HasIndex(l => l.Region);
        });

        modelBuilder.Entity<Location>().HasData(
            new Location 
            { 
                Id = 1, 
                Name = "Ancient Spire Outpost", 
                Type = "Outpost",
                Region = "The Wilds",
                HasMerchant = true,
                HasShipwright = true,
                HasWeaponsmith = true,
                HasTavern = true,
                IsActive = true
            },
            new Location 
            { 
                Id = 2, 
                Name = "Sanctuary Outpost", 
                Type = "Outpost",
                Region = "The Shores of Plenty",
                HasMerchant = true,
                HasShipwright = true,
                HasWeaponsmith = true,
                HasTavern = true,
                IsActive = true
            },
            new Location 
            { 
                Id = 3, 
                Name = "Dagger Tooth Outpost", 
                Type = "Outpost",
                Region = "The Wilds",
                HasMerchant = true,
                HasShipwright = true,
                HasWeaponsmith = true,
                HasTavern = true,
                IsActive = true
            },
            new Location 
            { 
                Id = 4, 
                Name = "Galleon's Grave Outpost", 
                Type = "Outpost",
                Region = "The Wilds",
                HasMerchant = true,
                HasShipwright = true,
                HasWeaponsmith = true,
                HasTavern = true,
                IsActive = true
            },
            new Location 
            { 
                Id = 5, 
                Name = "Golden Sands Outpost", 
                Type = "Outpost",
                Region = "The Shores of Plenty",
                HasMerchant = true,
                HasShipwright = true,
                HasWeaponsmith = true,
                HasTavern = true,
                IsActive = true
            },
            new Location 
            { 
                Id = 6, 
                Name = "Plunder Outpost", 
                Type = "Outpost",
                Region = "The Shores of Plenty",
                HasMerchant = true,
                HasShipwright = true,
                HasWeaponsmith = true,
                HasTavern = true,
                IsActive = true
            },
            new Location 
            { 
                Id = 7, 
                Name = "Reaper's Hideout", 
                Type = "Hideout",
                Region = "The Devil's Roar",
                HasMerchant = false,
                HasShipwright = false,
                HasWeaponsmith = false,
                HasTavern = false,
                IsActive = true
            },
            new Location 
            { 
                Id = 8, 
                Name = "Morrow's Peak Outpost", 
                Type = "Outpost",
                Region = "The Devil's Roar",
                HasMerchant = true,
                HasShipwright = true,
                HasWeaponsmith = true,
                HasTavern = true,
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
