// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Persistence/DesignTimeDbContextFactory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NovelVision.Services.Visualization.Infrastructure.Persistence;

/// <summary>
/// Factory для создания DbContext во время design-time (миграции)
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VisualizationDbContext>
{
    public VisualizationDbContext CreateDbContext(string[] args)
    {
        // Путь к appsettings.json в API проекте
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..",
            "NovelVision.Services.Visualization.API");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(basePath) ? basePath : Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("VisualizationDb")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Server=LAPTOP-OHNFI7TT;Database=NovelVision.Visualization;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<VisualizationDbContext>();

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(VisualizationDbContext).Assembly.FullName);
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "visualization");
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        return new VisualizationDbContext(optionsBuilder.Options);
    }
}