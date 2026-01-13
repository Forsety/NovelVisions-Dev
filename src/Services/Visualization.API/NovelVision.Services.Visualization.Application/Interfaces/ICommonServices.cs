using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Interfaces;

/// <summary>
/// Интерфейс для background job service
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Запустить обработку задания
    /// </summary>
    Task EnqueueProcessJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Запланировать отложенную обработку
    /// </summary>
    Task ScheduleProcessJobAsync(
        VisualizationJobId jobId,
        TimeSpan delay,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отменить запланированное задание
    /// </summary>
    Task CancelScheduledJobAsync(
        string scheduledJobId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Интерфейс для работы с текущим пользователем
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID текущего пользователя
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Email текущего пользователя
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Роли текущего пользователя
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Является ли пользователь администратором
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Аутентифицирован ли пользователь
    /// </summary>
    bool IsAuthenticated { get; }
}

/// <summary>
/// Интерфейс для работы с датой/временем (для тестируемости)
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Текущее UTC время
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Текущее локальное время
    /// </summary>
    DateTime Now { get; }
}
