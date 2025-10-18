using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EventService.Models;

namespace EventService
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EventDb>
    {
        public EventDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventDb>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EventDb;Trusted_Connection=True;");
            return new EventDb(optionsBuilder.Options);
        }
    }
}
