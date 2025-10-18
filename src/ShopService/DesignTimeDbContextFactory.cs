using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShopService.Models;

namespace ShopService
{
    // Design-time factory to avoid executing Program.cs during `dotnet ef` operations
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShopDb>
    {
        public ShopDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ShopDb>();
            // Use a local SQL connection string that doesn't require contacting startup host
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ShopDb;Trusted_Connection=True;");
            return new ShopDb(optionsBuilder.Options);
        }
    }
}
