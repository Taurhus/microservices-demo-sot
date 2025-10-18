using Microsoft.EntityFrameworkCore;
namespace ShopService.Models
{
    public class ShopDb : DbContext
    {
        public ShopDb(DbContextOptions<ShopDb> options) : base(options) { }
        public DbSet<Shop> Shops => Set<Shop>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Shop>().HasData(
                new Shop { Id = 1, Name = "Shipwright" },
                new Shop { Id = 2, Name = "Weaponsmith" },
                new Shop { Id = 3, Name = "Clothing Shop" }
            );
        }
    }
}