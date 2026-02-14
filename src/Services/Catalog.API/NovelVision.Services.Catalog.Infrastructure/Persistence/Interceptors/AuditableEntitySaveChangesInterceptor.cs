using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Entities;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public AuditableEntitySaveChangesInterceptor(
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
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

        var now = _dateTime.UtcNow;
        var userId = _currentUserService.UserId;

        // Update Books
        foreach (var entry in context.ChangeTracker.Entries<Book>())
        {
            UpdateAuditFields(entry, now, userId);
        }

        // Update Authors
        foreach (var entry in context.ChangeTracker.Entries<Author>())
        {
            UpdateAuditFields(entry, now, userId);
        }

        // Update Chapters
        foreach (var entry in context.ChangeTracker.Entries<Chapter>())
        {
            UpdateAuditFields(entry, now, userId);
        }

        // Update Pages
        foreach (var entry in context.ChangeTracker.Entries<Page>())
        {
            UpdateAuditFields(entry, now, userId);
        }
    }

    private void UpdateAuditFields<T>(EntityEntry<T> entry, DateTime now, Guid? userId)
        where T : class
    {
        switch (entry.State)
        {
            case EntityState.Added:
                SetPropertyValue(entry.Entity, "CreatedAt", now);
                SetPropertyValue(entry.Entity, "UpdatedAt", now);
                if (userId.HasValue)
                {
                    SetPropertyValue(entry.Entity, "CreatedBy", userId.Value);
                    SetPropertyValue(entry.Entity, "UpdatedBy", userId.Value);
                }
                break;

            case EntityState.Modified:
                SetPropertyValue(entry.Entity, "UpdatedAt", now);
                if (userId.HasValue)
                {
                    SetPropertyValue(entry.Entity, "UpdatedBy", userId.Value);
                }
                // Ensure CreatedAt is not modified
                entry.Property("CreatedAt").IsModified = false;
                if (HasProperty(entry.Entity, "CreatedBy"))
                {
                    entry.Property("CreatedBy").IsModified = false;
                }
                break;
        }
    }

    private bool HasProperty(object entity, string propertyName)
    {
        return entity.GetType().GetProperty(propertyName) != null;
    }

    private void SetPropertyValue(object entity, string propertyName, object value)
    {
        var property = entity.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(entity, value);
        }
    }
}
