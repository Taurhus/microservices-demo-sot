using Microsoft.EntityFrameworkCore;

namespace ItemService.Models
{
    public class ItemDb : DbContext
    {
        public ItemDb(DbContextOptions<ItemDb> options) : base(options) { }
        public DbSet<Item> Items => Set<Item>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().HasData(
                new Item { Id = 1, Name = "Banana" },
                new Item { Id = 2, Name = "Wooden Plank" },
                new Item { Id = 3, Name = "Cannonball" }
            );
        }
    }
}
