using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.Events;

/// <summary>
/// Запрос на визуализацию создан
/// </summary>
public sealed record VisualizationRequestedEvent(
    VisualizationJobId JobId,
    Guid BookId,
    Guid? PageId,
    Guid? ChapterId,
    VisualizationTrigger Trigger,
    Guid UserId) : DomainEvent;

/// <summary>
/// Задание визуализации поставлено в очередь
/// </summary>
public sealed record VisualizationQueuedEvent(
    VisualizationJobId JobId,
    int QueuePosition,
    TimeSpan EstimatedWaitTime) : DomainEvent;

/// <summary>
/// Начата генерация промпта
/// </summary>
public sealed record PromptGenerationStartedEvent(
    VisualizationJobId JobId,
    string OriginalText,
    AIModelProvider TargetModel) : DomainEvent;

/// <summary>
/// Промпт успешно сгенерирован
/// </summary>
public sealed record PromptGeneratedEvent(
    VisualizationJobId JobId,
    string EnhancedPrompt,
    string? NegativePrompt) : DomainEvent;

/// <summary>
/// Начата обработка AI моделью
/// </summary>
public sealed record AIProcessingStartedEvent(
    VisualizationJobId JobId,
    AIModelProvider Provider,
    string Prompt) : DomainEvent;

/// <summary>
/// AI модель вернула результат
/// </summary>
public sealed record AIProcessingCompletedEvent(
    VisualizationJobId JobId,
    string ExternalJobId,
    TimeSpan ProcessingTime) : DomainEvent;

/// <summary>
/// Изображение успешно загружено в хранилище
/// </summary>
public sealed record ImageUploadedEvent(
    VisualizationJobId JobId,
    GeneratedImageId ImageId,
    string ImageUrl,
    string? ThumbnailUrl) : DomainEvent;

/// <summary>
/// Визуализация успешно завершена
/// </summary>
public sealed record VisualizationCompletedEvent(
    VisualizationJobId JobId,
    GeneratedImageId ImageId,
    Guid BookId,
    Guid? PageId,
    string ImageUrl,
    TimeSpan TotalProcessingTime) : DomainEvent;

/// <summary>
/// Ошибка при визуализации
/// </summary>
public sealed record VisualizationFailedEvent(
    VisualizationJobId JobId,
    string ErrorMessage,
    string? ErrorCode,
    VisualizationJobStatus FailedAtStatus,
    int RetryCount) : DomainEvent;

/// <summary>
/// Визуализация отменена
/// </summary>
public sealed record VisualizationCancelledEvent(
    VisualizationJobId JobId,
    string Reason,
    Guid? CancelledByUserId) : DomainEvent;

/// <summary>
/// Запущена повторная попытка
/// </summary>
public sealed record VisualizationRetryEvent(
    VisualizationJobId JobId,
    int AttemptNumber,
    string PreviousError) : DomainEvent;
