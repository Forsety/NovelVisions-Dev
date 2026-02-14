// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Queue/InMemoryJobQueueService.cs

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Queue;

/// <summary>
/// In-Memory реализация очереди заданий (для разработки)
/// </summary>
public sealed class InMemoryJobQueueService : IJobQueueService
{
    private readonly ILogger<InMemoryJobQueueService> _logger;

    // Thread-safe priority queue (SortedSet с кастомным comparer)
    private readonly SortedSet<QueueItem> _queue = new(new QueueItemComparer());
    private readonly object _lock = new();

    private const int AverageProcessingTimeSeconds = 20;

    public InMemoryJobQueueService(ILogger<InMemoryJobQueueService> logger)
    {
        _logger = logger;
    }

    public Task<int> EnqueueJobAsync(
        VisualizationJobId jobId,
        int priority,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = new QueueItem(jobId, priority, DateTime.UtcNow);
            _queue.Add(item);

            var position = GetPositionUnsafe(jobId);

            _logger.LogInformation(
                "Enqueued job {JobId} with priority {Priority} at position {Position}",
                jobId.Value, priority, position);

            return Task.FromResult(position);
        }
    }

    public Task<VisualizationJobId?> DequeueJobAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_queue.Count == 0)
                return Task.FromResult<VisualizationJobId?>(null);

            var first = _queue.Min;
            if (first == null)
                return Task.FromResult<VisualizationJobId?>(null);

            _queue.Remove(first);

            _logger.LogDebug("Dequeued job {JobId}", first.JobId.Value);
            return Task.FromResult<VisualizationJobId?>(first.JobId);
        }
    }

    public Task<int> GetQueuePositionAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(GetPositionUnsafe(jobId));
        }
    }

    public Task<int> GetQueueLengthAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult(_queue.Count);
        }
    }

    public Task<bool> RemoveFromQueueAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var item = _queue.FirstOrDefault(i => i.JobId == jobId);
            if (item == null)
                return Task.FromResult(false);

            var removed = _queue.Remove(item);

            if (removed)
            {
                _logger.LogInformation("Removed job {JobId} from queue", jobId.Value);
            }

            return Task.FromResult(removed);
        }
    }

    public Task<TimeSpan> EstimateWaitTimeAsync(
        int queuePosition,
        CancellationToken cancellationToken = default)
    {
        if (queuePosition <= 0)
            return Task.FromResult(TimeSpan.Zero);

        var estimatedSeconds = queuePosition * AverageProcessingTimeSeconds;
        return Task.FromResult(TimeSpan.FromSeconds(estimatedSeconds));
    }

    private int GetPositionUnsafe(VisualizationJobId jobId)
    {
        var position = 1;
        foreach (var item in _queue)
        {
            if (item.JobId == jobId)
                return position;
            position++;
        }
        return 0;
    }

    private sealed record QueueItem(VisualizationJobId JobId, int Priority, DateTime EnqueuedAt);

    private sealed class QueueItemComparer : IComparer<QueueItem>
    {
        public int Compare(QueueItem? x, QueueItem? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return 1;
            if (y == null) return -1;

            // Higher priority first (descending)
            var priorityCompare = y.Priority.CompareTo(x.Priority);
            if (priorityCompare != 0) return priorityCompare;

            // Earlier enqueued first (ascending)
            var timeCompare = x.EnqueuedAt.CompareTo(y.EnqueuedAt);
            if (timeCompare != 0) return timeCompare;

            // Tie-breaker by ID to ensure uniqueness
            return x.JobId.Value.CompareTo(y.JobId.Value);
        }
    }
}