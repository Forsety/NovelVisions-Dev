// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Persistence/Repositories/GeneratedImageRepository.cs

using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для GeneratedImage (Read-only)
/// </summary>
public sealed class GeneratedImageRepository : IGeneratedImageRepository
{
    private readonly VisualizationDbContext _context;

    public GeneratedImageRepository(VisualizationDbContext context)
    {
        _context = context;
    }

    public async Task<GeneratedImage?> GetByIdAsync(
        GeneratedImageId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.GeneratedImages
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<GeneratedImage>> GetByJobIdAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        return await _context.GeneratedImages
            .Where(i => i.JobId == jobId && !i.IsDeleted)
            .OrderByDescending(i => i.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GeneratedImage>> GetByBookIdAsync(
        Guid bookId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        // Join через VisualizationJob для получения по BookId
        return await _context.GeneratedImages
            .Join(
                _context.VisualizationJobs,
                image => image.JobId,
                job => job.Id,
                (image, job) => new { Image = image, Job = job })
            .Where(x => x.Job.BookId == bookId && !x.Image.IsDeleted)
            .OrderByDescending(x => x.Image.GeneratedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => x.Image)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GeneratedImage>> GetByPageIdAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.GeneratedImages
            .Join(
                _context.VisualizationJobs,
                image => image.JobId,
                job => job.Id,
                (image, job) => new { Image = image, Job = job })
            .Where(x => x.Job.PageId == pageId && !x.Image.IsDeleted)
            .OrderByDescending(x => x.Image.GeneratedAt)
            .Select(x => x.Image)
            .ToListAsync(cancellationToken);
    }

    public async Task<GeneratedImage?> GetSelectedForPageAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.GeneratedImages
            .Join(
                _context.VisualizationJobs,
                image => image.JobId,
                job => job.Id,
                (image, job) => new { Image = image, Job = job })
            .Where(x => x.Job.PageId == pageId && x.Image.IsSelected && !x.Image.IsDeleted)
            .OrderByDescending(x => x.Image.GeneratedAt)
            .Select(x => x.Image)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetCountByBookIdAsync(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        return await _context.GeneratedImages
            .Join(
                _context.VisualizationJobs,
                image => image.JobId,
                job => job.Id,
                (image, job) => new { Image = image, Job = job })
            .CountAsync(x => x.Job.BookId == bookId && !x.Image.IsDeleted, cancellationToken);
    }
}