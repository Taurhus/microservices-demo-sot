using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShipService;

namespace ShipService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShipDb>
    {
        public ShipDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShipDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ShipDb;Trusted_Connection=True;");
            return new ShipDb(optionsBuilder.Options);
        }
    }
}
