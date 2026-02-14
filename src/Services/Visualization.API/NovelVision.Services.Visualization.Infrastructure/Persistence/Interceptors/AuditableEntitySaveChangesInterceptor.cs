// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Visualization.Application.Interfaces;

namespace NovelVision.Services.Visualization.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor для автоматического заполнения CreatedAt/UpdatedAt
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(
        IDateTimeProvider dateTimeProvider,
        ICurrentUserService currentUserService)
    {
        _dateTimeProvider = dateTimeProvider;
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var utcNow = _dateTimeProvider.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                SetPropertyValue(entry, "CreatedAt", utcNow);
                SetPropertyValue(entry, "UpdatedAt", utcNow);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetPropertyValue(entry, "UpdatedAt", utcNow);
            }
        }
    }

    private static void SetPropertyValue(EntityEntry entry, string propertyName, object value)
    {
        var property = entry.Property(propertyName);
        if (property != null && property.Metadata.PropertyInfo != null)
        {
            property.CurrentValue = value;
        }
    }
}