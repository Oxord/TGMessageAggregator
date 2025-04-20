using Microsoft.EntityFrameworkCore;
// Removed duplicate using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MessageAggregator.Infrastructure
{
    /// <summary>
    /// Factory for creating AppDbContext instances during design time (e.g., for EF Core Migrations).
    /// This is necessary because the DbContext is in a separate project from the startup project (WebApi).
    /// It manually builds the configuration to find the connection string.
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            // Adjust the path based on where the command is executed relative to the appsettings.json file.
            // Assuming the command is run from the solution root or WebApi project.
            // If run from MessageAggregator, the path needs to go up one level and into WebApi.
            // Let's try a path relative to the solution root first.
            string basePath = Directory.GetCurrentDirectory();
            // Heuristic to find the solution root or adjust path if needed
            if (!File.Exists(Path.Combine(basePath, "WebApi", "appsettings.Development.json")))
            {
                 // If not found relative to CWD, assume CWD is MessageAggregator and go up/over
                 basePath = Path.GetFullPath(Path.Combine(basePath, ".."));
            }


            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(basePath, "WebApi")) // Point to the WebApi directory
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true) // Load development settings
                .AddEnvironmentVariables()
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Could not find 'DefaultConnection' connection string in appsettings.Development.json");
            }

            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString, opts => opts.MigrationsAssembly("MessageAggregator")); // Specify migrations assembly

            // Return new instance of AppDbContext
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
