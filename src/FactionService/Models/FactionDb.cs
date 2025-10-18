using Microsoft.EntityFrameworkCore;

namespace FactionService.Models
{
    public class FactionDb : DbContext
    {
        public FactionDb(DbContextOptions<FactionDb> options) : base(options) { }
        public DbSet<Faction> Factions => Set<Faction>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Faction>().HasData(
                new Faction { Id = 1, Name = "Gold Hoarders" },
                new Faction { Id = 2, Name = "Order of Souls" },
                new Faction { Id = 3, Name = "Merchant Alliance" },
                new Faction { Id = 4, Name = "Reaper's Bones" }
            );
        }
    }
}
