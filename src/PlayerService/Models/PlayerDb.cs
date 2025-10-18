using Microsoft.EntityFrameworkCore;

namespace PlayerService.Models
{
    public class PlayerDb : DbContext
    {
        public PlayerDb(DbContextOptions<PlayerDb> options) : base(options) { }
        public DbSet<Player> Players => Set<Player>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>().HasData(
                new Player { Id = 1, Name = "Captain Flameheart" },
                new Player { Id = 2, Name = "Merrick" },
                new Player { Id = 3, Name = "Umbra" }
            );
        }
    }
}
