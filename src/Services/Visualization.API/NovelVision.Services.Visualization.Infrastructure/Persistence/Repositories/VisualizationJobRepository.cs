// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Persistence/Repositories/VisualizationJobRepository.cs

using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для VisualizationJob
/// </summary>
public sealed class VisualizationJobRepository : IVisualizationJobRepository
{
    private readonly VisualizationDbContext _context;

    public VisualizationJobRepository(VisualizationDbContext context)
    {
        _context = context;
    }

    #region IVisualizationJobRepository Implementation

    public async Task<VisualizationJob?> GetByIdAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.VisualizationJobs
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<VisualizationJob?> GetByIdWithImagesAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.VisualizationJobs
            .Include(j => j.Images)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<VisualizationJob>> GetByBookIdAsync(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        return await _context.VisualizationJobs
            .Where(j => j.BookId == bookId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VisualizationJob>> GetByPageIdAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.VisualizationJobs
            .Where(j => j.PageId == pageId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VisualizationJob>> GetByUserIdAsync(
        Guid userId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.VisualizationJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VisualizationJob>> GetByStatusAsync(
        VisualizationJobStatus status,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        // Используем EF.Property для доступа к backing field
        return await _context.VisualizationJobs
            .Where(j => EF.Property<int>(j, "_status") == status.Value)
            .OrderBy(j => j.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<VisualizationJob?> GetNextPendingAsync(
        CancellationToken cancellationToken = default)
    {
        // Получаем следующее задание из очереди (Queued статус, высший приоритет, самое старое)
        return await _context.VisualizationJobs
            .Where(j => EF.Property<int>(j, "_status") == VisualizationJobStatus.Queued.Value)
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.VisualizationJobs
            .AnyAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<int> GetQueueLengthAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.VisualizationJobs
            .CountAsync(j =>
                EF.Property<int>(j, "_status") == VisualizationJobStatus.Pending.Value ||
                EF.Property<int>(j, "_status") == VisualizationJobStatus.Queued.Value,
                cancellationToken);
    }

    public async Task<int> GetQueuePositionAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default)
    {
        var job = await GetByIdAsync(id, cancellationToken);
        if (job == null || job.Status.IsFinal)
            return 0;

        return await _context.VisualizationJobs
            .CountAsync(j =>
                (EF.Property<int>(j, "_status") == VisualizationJobStatus.Pending.Value ||
                 EF.Property<int>(j, "_status") == VisualizationJobStatus.Queued.Value) &&
                (j.Priority > job.Priority ||
                 (j.Priority == job.Priority && j.CreatedAt < job.CreatedAt)),
                cancellationToken) + 1;
    }

    #endregion

    #region IRepository<VisualizationJob> Implementation

    public void Add(VisualizationJob entity)
    {
        _context.VisualizationJobs.Add(entity);
    }

    public void Update(VisualizationJob entity)
    {
        _context.VisualizationJobs.Update(entity);
    }

    public void Remove(VisualizationJob entity)
    {
        _context.VisualizationJobs.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}