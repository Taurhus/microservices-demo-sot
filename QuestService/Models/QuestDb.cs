using Microsoft.EntityFrameworkCore;

namespace QuestService.Models
{
    public class QuestDb : DbContext
    {
        public QuestDb(DbContextOptions<QuestDb> options) : base(options) { }
        public DbSet<Quest> Quests => Set<Quest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quest>().HasData(
                new Quest { Id = 1, Name = "The Shroudbreaker" },
                new Quest { Id = 2, Name = "The Cursed Rogue" },
                new Quest { Id = 3, Name = "The Legendary Storyteller" }
            );
        }
    }
}
