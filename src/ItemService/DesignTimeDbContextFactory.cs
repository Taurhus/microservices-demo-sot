using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ItemService.Models;

namespace ItemService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ItemDb>
    {
        public ItemDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ItemDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ItemDb;Trusted_Connection=True;");
            return new ItemDb(optionsBuilder.Options);
        }
    }
}
