using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Airline.DataAccess
{
    public class AirlineDbContextFactory : IDesignTimeDbContextFactory<AirlineDbContext>
    {
        public AirlineDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AirlineDbContext>();
            optionsBuilder.UseSqlite("Data Source=airline.db"); // or use SQL Server if preferred

            return new AirlineDbContext(optionsBuilder.Options);
        }
    }
}
