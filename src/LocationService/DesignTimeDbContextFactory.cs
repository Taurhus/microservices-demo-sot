using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using LocationService.Models;

namespace LocationService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LocationDb>
    {
        public LocationDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LocationDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LocationDb;Trusted_Connection=True;");
            return new LocationDb(optionsBuilder.Options);
        }
    }
}
