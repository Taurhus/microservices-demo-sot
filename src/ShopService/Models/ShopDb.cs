using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace ShopService.Models;

public class ShopDb : DbContext
{
    public ShopDb(DbContextOptions<ShopDb> options) : base(options) { }
    
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Type).HasMaxLength(50);
            entity.Property(s => s.LocationName).HasMaxLength(200);
            entity.Property(s => s.Description).HasMaxLength(500);
            entity.HasIndex(s => s.Name);
            entity.HasIndex(s => s.Type);
            entity.HasIndex(s => s.LocationName);
        });

        modelBuilder.Entity<Shop>().HasData(
            new Shop 
            { 
                Id = 1, 
                Name = "Shipwright", 
                Type = "Shipwright",
                Description = "A shop where players can customize their ship's appearance with hulls, sails, figureheads, and more.",
                LocationName = "All Outposts",
                IsActive = true
            },
            new Shop 
            { 
                Id = 2, 
                Name = "Weaponsmith", 
                Type = "Weaponsmith",
                Description = "A shop where players can purchase and customize weapons like swords, pistols, and blunderbusses.",
                LocationName = "All Outposts",
                IsActive = true
            },
            new Shop 
            { 
                Id = 3, 
                Name = "Clothing Shop", 
                Type = "Clothing Shop",
                Description = "A shop where players can purchase clothing, accessories, and cosmetic items for their pirate.",
                LocationName = "All Outposts",
                IsActive = true
            },
            new Shop 
            { 
                Id = 4, 
                Name = "Pirate Emporium", 
                Type = "Premium Shop",
                Description = "An in-game store where players can purchase premium cosmetics and items using Ancient Coins.",
                LocationName = "All Outposts",
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