using Microsoft.EntityFrameworkCore;

namespace EventService.Models
{
    public class EventDb : DbContext
    {
        public EventDb(DbContextOptions<EventDb> options) : base(options) { }
        public DbSet<EventEntity> Events => Set<EventEntity>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventEntity>().HasData(
                new EventEntity { Id = 1, Name = "Skeleton Fort" },
                new EventEntity { Id = 2, Name = "Ashen Winds" },
                new EventEntity { Id = 3, Name = "Fort of the Damned" }
            );
        }
    }
}
