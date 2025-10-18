using Microsoft.EntityFrameworkCore;

namespace LocationService.Models
{
    public class LocationDb : DbContext
    {
        public LocationDb(DbContextOptions<LocationDb> options) : base(options) { }
        public DbSet<Location> Locations => Set<Location>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Location>().HasData(
                new Location { Id = 1, Name = "Ancient Spire Outpost" },
                new Location { Id = 2, Name = "Sanctuary Outpost" },
                new Location { Id = 3, Name = "Dagger Tooth Outpost" }
            );
        }
    }
}
