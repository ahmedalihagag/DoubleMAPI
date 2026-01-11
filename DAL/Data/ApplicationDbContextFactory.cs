using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build path to the startup project (DoubleMAPI)
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "DoubleMAPI");
            
            // If running from solution root, try current directory first
            if (!Directory.Exists(basePath))
            {
                basePath = Path.Combine(Directory.GetCurrentDirectory(), "DoubleMAPI");
            }
            
            // Fallback to just using a hardcoded connection string if file not found
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                basePath = Directory.GetCurrentDirectory();
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
