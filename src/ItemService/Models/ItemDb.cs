using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace ItemService.Models;

public class ItemDb : DbContext
{
    public ItemDb(DbContextOptions<ItemDb> options) : base(options) { }
    
    public DbSet<Item> Items => Set<Item>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Description).HasMaxLength(500);
            entity.Property(i => i.Category).HasMaxLength(50);
            entity.Property(i => i.Rarity).HasMaxLength(50);
            entity.HasIndex(i => i.Name);
            entity.HasIndex(i => i.Category);
            entity.HasIndex(i => i.Rarity);
        });

        modelBuilder.Entity<Item>().HasData(
            new Item 
            { 
                Id = 1, 
                Name = "Banana", 
                Description = "A yellow fruit that restores health when consumed.",
                Category = "Consumable",
                Rarity = "Common",
                BaseValue = 0,
                IsStackable = true,
                MaxStackSize = 5,
                IsActive = true
            },
            new Item 
            { 
                Id = 2, 
                Name = "Wooden Plank", 
                Description = "Used to repair holes in your ship's hull.",
                Category = "Equipment",
                Rarity = "Common",
                BaseValue = 0,
                IsStackable = true,
                MaxStackSize = 10,
                IsActive = true
            },
            new Item 
            { 
                Id = 3, 
                Name = "Cannonball", 
                Description = "Ammunition for ship cannons. Deals damage to ships and structures.",
                Category = "Ammunition",
                Rarity = "Common",
                BaseValue = 0,
                IsStackable = true,
                MaxStackSize = 10,
                IsActive = true
            },
            new Item 
            { 
                Id = 4, 
                Name = "Castaway's Chest", 
                Description = "A basic treasure chest containing gold and trinkets.",
                Category = "Treasure",
                Rarity = "Common",
                BaseValue = 100,
                IsStackable = false,
                MaxStackSize = null,
                IsActive = true
            },
            new Item 
            { 
                Id = 5, 
                Name = "Marauder's Chest", 
                Description = "A valuable treasure chest worth more than a Castaway's Chest.",
                Category = "Treasure",
                Rarity = "Rare",
                BaseValue = 500,
                IsStackable = false,
                MaxStackSize = null,
                IsActive = true
            },
            new Item 
            { 
                Id = 6, 
                Name = "Captain's Chest", 
                Description = "A highly valuable treasure chest sought after by the Gold Hoarders.",
                Category = "Treasure",
                Rarity = "Rare",
                BaseValue = 1000,
                IsStackable = false,
                MaxStackSize = null,
                IsActive = true
            },
            new Item 
            { 
                Id = 7, 
                Name = "Villainous Skull", 
                Description = "A skull from a defeated skeleton captain. Valued by the Order of Souls.",
                Category = "Treasure",
                Rarity = "Rare",
                BaseValue = 1500,
                IsStackable = false,
                MaxStackSize = null,
                IsActive = true
            },
            new Item 
            { 
                Id = 8, 
                Name = "Stronghold Gunpowder Barrel", 
                Description = "An explosive barrel from a skeleton fort. Extremely dangerous but valuable.",
                Category = "Treasure",
                Rarity = "Legendary",
                BaseValue = 2500,
                IsStackable = false,
                MaxStackSize = null,
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
