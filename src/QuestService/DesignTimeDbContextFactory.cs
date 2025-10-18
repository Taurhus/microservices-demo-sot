using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuestService.Models;

namespace QuestService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<QuestDb>
    {
        public QuestDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QuestDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=QuestDb;Trusted_Connection=True;");
            return new QuestDb(optionsBuilder.Options);
        }
    }
}
