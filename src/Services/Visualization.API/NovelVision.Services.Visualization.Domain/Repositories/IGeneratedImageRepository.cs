// Repositories/IGeneratedImageRepository.cs
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.Repositories;

/// <summary>
/// Репозиторий для сгенерированных изображений (Read-only)
/// GeneratedImage - Entity, не AggregateRoot, поэтому не наследуем IReadRepository
/// </summary>
public interface IGeneratedImageRepository
{
    /// <summary>
    /// Получить изображение по ID
    /// </summary>
    Task<GeneratedImage?> GetByIdAsync(
        GeneratedImageId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить изображения по заданию
    /// </summary>
    Task<IReadOnlyList<GeneratedImage>> GetByJobIdAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить изображения по книге
    /// </summary>
    Task<IReadOnlyList<GeneratedImage>> GetByBookIdAsync(
        Guid bookId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить изображения по странице
    /// </summary>
    Task<IReadOnlyList<GeneratedImage>> GetByPageIdAsync(
        Guid pageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить выбранное изображение для страницы
    /// </summary>
    Task<GeneratedImage?> GetSelectedForPageAsync(
        Guid pageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить общее количество для книги
    /// </summary>
    Task<int> GetCountByBookIdAsync(
        Guid bookId,
        CancellationToken cancellationToken = default);
}