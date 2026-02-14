// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Persistence/VisualizationDbContext.cs

using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using System.Reflection;

namespace NovelVision.Services.Visualization.Infrastructure.Persistence;

/// <summary>
/// DbContext для Visualization микросервиса
/// </summary>
public sealed class VisualizationDbContext : DbContext
{
    public VisualizationDbContext(DbContextOptions<VisualizationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Задания визуализации
    /// </summary>
    public DbSet<VisualizationJob> VisualizationJobs => Set<VisualizationJob>();

    /// <summary>
    /// Сгенерированные изображения
    /// </summary>
    public DbSet<GeneratedImage> GeneratedImages => Set<GeneratedImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Применяем все конфигурации из текущей сборки
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Устанавливаем схему для таблиц
        modelBuilder.HasDefaultSchema("visualization");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Глобальные конвенции для строк
        configurationBuilder.Properties<string>()
            .HaveMaxLength(500);

        // Конвенции для DateTime - всегда UTC
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<DateTimeUtcConverter>();
    }
}

/// <summary>
/// Конвертер для обеспечения UTC времени
/// </summary>
public sealed class DateTimeUtcConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>
{
    public DateTimeUtcConverter()
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}