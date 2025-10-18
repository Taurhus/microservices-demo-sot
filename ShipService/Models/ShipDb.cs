
using Microsoft.EntityFrameworkCore;
using ShipService.Models;

namespace ShipService
{
    public class ShipDb : DbContext
    {
        public ShipDb(DbContextOptions<ShipDb> options) : base(options) { }
        public DbSet<Ship> Ships => Set<Ship>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ship>().HasData(
                new Ship { Id = 1, Name = "Sloop" },
                new Ship { Id = 2, Name = "Brigantine" },
                new Ship { Id = 3, Name = "Galleon" }
            );
        }
    }
}
