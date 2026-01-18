// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Cache/InMemoryCacheService.cs

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Cache;

/// <summary>
/// In-Memory реализация кэш-сервиса (для разработки)
/// </summary>
public sealed class InMemoryCacheService : IVisualizationCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheService> _logger;

    private const string JobKeyPrefix = "visualization:job:";
    private const string QueueStatusKey = "visualization:queue:status";

    public InMemoryCacheService(
        IMemoryCache cache,
        ILogger<InMemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<VisualizationJobDto?> GetJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        var key = GetJobKey(jobId);
        _cache.TryGetValue(key, out VisualizationJobDto? job);
        return Task.FromResult(job);
    }

    public Task SetJobAsync(
        VisualizationJobDto job,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var key = GetJobKey(VisualizationJobId.From(job.Id));
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(60)
        };

        _cache.Set(key, job, options);
        _logger.LogDebug("Cached job {JobId} in memory", job.Id);

        return Task.CompletedTask;
    }

    public Task InvalidateJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        var key = GetJobKey(jobId);
        _cache.Remove(key);
        _logger.LogDebug("Invalidated cache for job {JobId}", jobId.Value);

        return Task.CompletedTask;
    }

    public Task<QueueStatusDto?> GetQueueStatusAsync(
        CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(QueueStatusKey, out QueueStatusDto? status);
        return Task.FromResult(status);
    }

    public Task SetQueueStatusAsync(
        QueueStatusDto status,
        CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        };

        _cache.Set(QueueStatusKey, status, options);
        return Task.CompletedTask;
    }

    private static string GetJobKey(VisualizationJobId jobId) => $"{JobKeyPrefix}{jobId.Value}";
}