using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Infrastructure;

namespace CrewService.Models;

public class CrewDb : DbContext
{
    public CrewDb(DbContextOptions<CrewDb> options) : base(options) { }
    
    public DbSet<Crew> Crews => Set<Crew>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Crew>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Status).HasMaxLength(50);
            entity.Property(c => c.Notes).HasMaxLength(500);
            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => c.ShipId);
            entity.HasIndex(c => c.Status);
        });

        modelBuilder.Entity<Crew>().HasData(
            new Crew 
            { 
                Id = 1, 
                Name = "The Sea Dogs",
                ShipId = 3, // Galleon
                MaxMembers = 4,
                CurrentMembers = 4,
                Status = "Active",
                CreatedDate = new DateTime(2024, 12, 10, 10, 30, 0, DateTimeKind.Utc),
                LastActivityDate = new DateTime(2024, 12, 15, 10, 30, 0, DateTimeKind.Utc),
                Notes = "Experienced galleon crew"
            },
            new Crew 
            { 
                Id = 2, 
                Name = "Solo Sailors",
                ShipId = 1, // Sloop
                MaxMembers = 2,
                CurrentMembers = 1,
                Status = "Active",
                CreatedDate = new DateTime(2024, 12, 13, 10, 30, 0, DateTimeKind.Utc),
                LastActivityDate = new DateTime(2024, 12, 15, 9, 30, 0, DateTimeKind.Utc),
                Notes = "Solo player crew"
            },
            new Crew 
            { 
                Id = 3, 
                Name = "The Brigands",
                ShipId = 2, // Brigantine
                MaxMembers = 3,
                CurrentMembers = 3,
                Status = "Active",
                CreatedDate = new DateTime(2024, 12, 14, 10, 30, 0, DateTimeKind.Utc),
                LastActivityDate = new DateTime(2024, 12, 15, 10, 30, 0, DateTimeKind.Utc),
                Notes = "Three-person brigantine crew"
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

