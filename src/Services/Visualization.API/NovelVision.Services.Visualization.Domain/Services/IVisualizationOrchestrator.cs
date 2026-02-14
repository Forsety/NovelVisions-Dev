using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Domain.Services;

/// <summary>
/// Интерфейс оркестратора визуализации
/// Координирует взаимодействие между Catalog.API, PromptGen.API и AI провайдерами
/// </summary>
public interface IVisualizationOrchestrator
{
    /// <summary>
    /// Запросить визуализацию страницы (кнопка "Визуализируй")
    /// </summary>
    Task<Result<VisualizationJob>> RequestPageVisualizationAsync(
        Guid bookId,
        Guid pageId,
        Guid userId,
        AIModelProvider? preferredProvider = null,
        GenerationParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Запросить визуализацию выделенного текста
    /// </summary>
    Task<Result<VisualizationJob>> RequestTextSelectionVisualizationAsync(
        Guid bookId,
        TextSelection textSelection,
        Guid userId,
        AIModelProvider? preferredProvider = null,
        GenerationParameters? parameters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Запустить авто веб-новеллу для книги
    /// </summary>
    Task<Result<IReadOnlyList<VisualizationJob>>> StartAutoNovelGenerationAsync(
        Guid bookId,
        Guid userId,
        AIModelProvider? preferredProvider = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обработать задание (вызывается Background Worker)
    /// </summary>
    Task<Result<bool>> ProcessJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить статус задания
    /// </summary>
    Task<Result<VisualizationJobStatus>> GetJobStatusAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отменить задание
    /// </summary>
    Task<Result<bool>> CancelJobAsync(
        VisualizationJobId jobId,
        Guid userId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Повторить неудачное задание
    /// </summary>
    Task<Result<bool>> RetryJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Регенерировать изображение
    /// </summary>
    Task<Result<VisualizationJob>> RegenerateAsync(
        VisualizationJobId originalJobId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
