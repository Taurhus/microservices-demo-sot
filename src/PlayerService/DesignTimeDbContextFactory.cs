using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PlayerService.Models;

namespace PlayerService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PlayerDb>
    {
        public PlayerDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PlayerDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=PlayerDb;Trusted_Connection=True;");
            return new PlayerDb(optionsBuilder.Options);
        }
    }
}
