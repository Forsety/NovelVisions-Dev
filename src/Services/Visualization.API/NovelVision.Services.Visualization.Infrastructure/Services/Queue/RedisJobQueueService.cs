// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Queue/RedisJobQueueService.cs

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Infrastructure.Settings;
using StackExchange.Redis;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Queue;

/// <summary>
/// Redis реализация очереди заданий с приоритетами
/// Использует Sorted Set для приоритетной очереди
/// </summary>
public sealed class RedisJobQueueService : IJobQueueService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisJobQueueService> _logger;

    // Среднее время обработки одного задания (секунды)
    private const int AverageProcessingTimeSeconds = 20;

    public RedisJobQueueService(
        IConnectionMultiplexer redis,
        IOptions<RedisSettings> settings,
        ILogger<RedisJobQueueService> logger)
    {
        _redis = redis;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<int> EnqueueJobAsync(
        VisualizationJobId jobId,
        int priority,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var queueKey = _settings.PriorityQueueName;

            // Score = -priority (чтобы высший приоритет был первым) + timestamp/1000000 (для FIFO в пределах приоритета)
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var score = -priority + (timestamp / 1_000_000_000.0);

            await db.SortedSetAddAsync(queueKey, jobId.Value.ToString(), score);

            var position = await GetQueuePositionAsync(jobId, cancellationToken);

            _logger.LogInformation(
                "Enqueued job {JobId} with priority {Priority} at position {Position}",
                jobId.Value, priority, position);

            return position;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue job {JobId}", jobId.Value);
            throw;
        }
    }

    public async Task<VisualizationJobId?> DequeueJobAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var queueKey = _settings.PriorityQueueName;

            // Получаем и удаляем элемент с наименьшим score (высший приоритет)
            var result = await db.SortedSetPopAsync(queueKey, Order.Ascending);

            if (!result.HasValue)
                return null;

            var jobIdStr = result.Value.Element.ToString();
            if (Guid.TryParse(jobIdStr, out var jobId))
            {
                _logger.LogDebug("Dequeued job {JobId}", jobId);
                return VisualizationJobId.From(jobId);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dequeue job");
            return null;
        }
    }

    public async Task<int> GetQueuePositionAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var queueKey = _settings.PriorityQueueName;

            // Rank возвращает 0-based позицию
            var rank = await db.SortedSetRankAsync(queueKey, jobId.Value.ToString(), Order.Ascending);

            return rank.HasValue ? (int)rank.Value + 1 : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get queue position for job {JobId}", jobId.Value);
            return 0;
        }
    }

    public async Task<int> GetQueueLengthAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var queueKey = _settings.PriorityQueueName;

            return (int)await db.SortedSetLengthAsync(queueKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get queue length");
            return 0;
        }
    }

    public async Task<bool> RemoveFromQueueAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var queueKey = _settings.PriorityQueueName;

            var removed = await db.SortedSetRemoveAsync(queueKey, jobId.Value.ToString());

            if (removed)
            {
                _logger.LogInformation("Removed job {JobId} from queue", jobId.Value);
            }

            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove job {JobId} from queue", jobId.Value);
            return false;
        }
    }

    public Task<TimeSpan> EstimateWaitTimeAsync(
        int queuePosition,
        CancellationToken cancellationToken = default)
    {
        if (queuePosition <= 0)
            return Task.FromResult(TimeSpan.Zero);

        // Простая оценка: позиция * среднее время обработки
        var estimatedSeconds = queuePosition * AverageProcessingTimeSeconds;

        return Task.FromResult(TimeSpan.FromSeconds(estimatedSeconds));
    }
}