using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Interfaces;

/// <summary>
/// Интерфейс для отправки уведомлений о прогрессе (SignalR)
/// </summary>
public interface IVisualizationNotificationService
{
    /// <summary>
    /// Уведомить о прогрессе задания
    /// </summary>
    Task NotifyJobProgressAsync(
        Guid userId,
        JobProgressDto progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомить о завершении задания
    /// </summary>
    Task NotifyJobCompletedAsync(
        Guid userId,
        VisualizationJobDto job,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомить об ошибке
    /// </summary>
    Task NotifyJobFailedAsync(
        Guid userId,
        Guid jobId,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомить об обновлении очереди
    /// </summary>
    Task NotifyQueueUpdateAsync(
        Guid userId,
        QueueStatusDto queueStatus,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Интерфейс для управления очередью заданий
/// </summary>
public interface IJobQueueService
{
    /// <summary>
    /// Добавить задание в очередь
    /// </summary>
    Task<int> EnqueueJobAsync(
        VisualizationJobId jobId,
        int priority,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить следующее задание из очереди
    /// </summary>
    Task<VisualizationJobId?> DequeueJobAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить позицию в очереди
    /// </summary>
    Task<int> GetQueuePositionAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить длину очереди
    /// </summary>
    Task<int> GetQueueLengthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить задание из очереди
    /// </summary>
    Task<bool> RemoveFromQueueAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Рассчитать примерное время ожидания
    /// </summary>
    Task<TimeSpan> EstimateWaitTimeAsync(
        int queuePosition,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Интерфейс для кэширования
/// </summary>
public interface IVisualizationCacheService
{
    /// <summary>
    /// Получить кэшированное задание
    /// </summary>
    Task<VisualizationJobDto?> GetJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Кэшировать задание
    /// </summary>
    Task SetJobAsync(
        VisualizationJobDto job,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Инвалидировать кэш задания
    /// </summary>
    Task InvalidateJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить статистику очереди из кэша
    /// </summary>
    Task<QueueStatusDto?> GetQueueStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить статистику очереди в кэше
    /// </summary>
    Task SetQueueStatusAsync(
        QueueStatusDto status,
        CancellationToken cancellationToken = default);
}
