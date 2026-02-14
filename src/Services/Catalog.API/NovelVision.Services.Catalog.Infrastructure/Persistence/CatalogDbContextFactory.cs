using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using NovelVision.Services.Catalog.Infrastructure.Identity.Persistence;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence;

/// <summary>
/// Factory for creating CatalogDbContext at design time for migrations
/// </summary>
public class CatalogDbContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();

        // Try to get configuration
        IConfigurationRoot configuration = null;
        try
        {
            var basePath = Directory.GetCurrentDirectory();
            // Try to find appsettings.json in API project
            var apiProjectPath = Path.Combine(basePath, "..", "NovelVision.Services.Catalog.API");
            if (Directory.Exists(apiProjectPath))
            {
                basePath = apiProjectPath;
            }

            configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
        }
        catch
        {
            // If configuration fails, use default connection string
        }

        var connectionString = configuration?.GetConnectionString("DefaultConnection")
            ?? "Data Source=LAPTOP-OHNFI7TT;Initial Catalog=NovelVisionCatalog;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        optionsBuilder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName));

        // Pass null for IMediator since it's not needed for migrations
        return new CatalogDbContext(optionsBuilder.Options, null);
    }
}
