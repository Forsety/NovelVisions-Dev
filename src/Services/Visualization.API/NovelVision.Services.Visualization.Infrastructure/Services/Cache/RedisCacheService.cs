// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Cache/RedisCacheService.cs

using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Infrastructure.Settings;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Cache;

/// <summary>
/// Redis реализация кэш-сервиса
/// </summary>
public sealed class RedisCacheService : IVisualizationCacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string JobKeyPrefix = "job:";
    private const string QueueStatusKey = "queue:status";

    public RedisCacheService(
        IDistributedCache cache,
        IOptions<RedisSettings> settings,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<VisualizationJobDto?> GetJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetJobKey(jobId);
            var cached = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cached))
                return null;

            return JsonSerializer.Deserialize<VisualizationJobDto>(cached, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get job {JobId} from cache", jobId.Value);
            return null;
        }
    }

    public async Task SetJobAsync(
        VisualizationJobDto job,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetJobKey(VisualizationJobId.From(job.Id));
            var json = JsonSerializer.Serialize(job, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_settings.JobCacheMinutes)
            };

            await _cache.SetStringAsync(key, json, options, cancellationToken);

            _logger.LogDebug("Cached job {JobId}", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache job {JobId}", job.Id);
        }
    }

    public async Task InvalidateJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetJobKey(jobId);
            await _cache.RemoveAsync(key, cancellationToken);

            _logger.LogDebug("Invalidated cache for job {JobId}", jobId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for job {JobId}", jobId.Value);
        }
    }

    public async Task<QueueStatusDto?> GetQueueStatusAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await _cache.GetStringAsync(QueueStatusKey, cancellationToken);

            if (string.IsNullOrEmpty(cached))
                return null;

            return JsonSerializer.Deserialize<QueueStatusDto>(cached, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get queue status from cache");
            return null;
        }
    }

    public async Task SetQueueStatusAsync(
        QueueStatusDto status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(status, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_settings.QueueCacheSeconds)
            };

            await _cache.SetStringAsync(QueueStatusKey, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache queue status");
        }
    }

    private string GetJobKey(VisualizationJobId jobId) => $"{JobKeyPrefix}{jobId.Value}";
}