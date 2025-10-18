using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using FactionService.Models;

namespace FactionService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FactionDb>
    {
        public FactionDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FactionDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=FactionDb;Trusted_Connection=True;");
            return new FactionDb(optionsBuilder.Options);
        }
    }
}
