using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure;

namespace FactionService.Models;

public class FactionDb : DbContext
{
    public FactionDb(DbContextOptions<FactionDb> options) : base(options) { }
    
    public DbSet<Faction> Factions => Set<Faction>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Faction>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(f => f.Name);
        });

        modelBuilder.Entity<Faction>().HasData(
            new Faction 
            { 
                Id = 1, 
                Name = "Gold Hoarders", 
                Description = "A trading company focused on treasure hunting and gold collection. They offer voyages involving maps and riddles to find buried treasure.",
                Type = "Trading Company",
                Headquarters = "Various Outposts",
                MaxReputationLevel = 75,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 3, 20)
            },
            new Faction 
            { 
                Id = 2, 
                Name = "Order of Souls", 
                Description = "A trading company specializing in combat against skeleton crews. They offer bounties on skeleton captains and their crews.",
                Type = "Trading Company",
                Headquarters = "Various Outposts",
                MaxReputationLevel = 75,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 3, 20)
            },
            new Faction 
            { 
                Id = 3, 
                Name = "Merchant Alliance", 
                Description = "A trading company focused on cargo delivery missions. They require timely transport of goods between outposts.",
                Type = "Trading Company",
                Headquarters = "Various Outposts",
                MaxReputationLevel = 75,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 3, 20)
            },
            new Faction 
            { 
                Id = 4, 
                Name = "Reaper's Bones", 
                Description = "A faction that rewards players for stealing and turning in treasure from other crews. They operate from The Reaper's Hideout.",
                Type = "Faction",
                Headquarters = "Reaper's Hideout",
                MaxReputationLevel = 75,
                IsActive = true,
                IntroducedDate = new DateTime(2020, 4, 22)
            },
            new Faction 
            { 
                Id = 5, 
                Name = "Hunter's Call", 
                Description = "A trading company focused on fishing and cooking. They reward players for catching fish and cooking meat.",
                Type = "Trading Company",
                Headquarters = "Seaposts",
                MaxReputationLevel = 50,
                IsActive = true,
                IntroducedDate = new DateTime(2019, 4, 30)
            },
            new Faction 
            { 
                Id = 6, 
                Name = "Smugglers' League", 
                Description = "A secretive faction introduced in Season 17 (August 2025) offering high-risk, high-reward smuggling voyages.",
                Type = "Faction",
                Headquarters = "Hidden Locations",
                MaxReputationLevel = 50,
                IsActive = true,
                IntroducedDate = new DateTime(2025, 8, 14)
            },
            new Faction 
            { 
                Id = 7, 
                Name = "Athena's Fortune", 
                Description = "An exclusive trading company for Pirate Legends. Requires reaching level 50 in three trading companies to unlock.",
                Type = "Trading Company",
                Headquarters = "The Tavern of Legends",
                MaxReputationLevel = 30,
                IsActive = true,
                IntroducedDate = new DateTime(2018, 9, 19)
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
