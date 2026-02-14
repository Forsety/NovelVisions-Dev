// src/Services/Visualization.API/NovelVision.Services.Visualization.Domain/Repositories/IVisualizationJobRepository.cs

using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.Repositories;

/// <summary>
/// Репозиторий для заданий визуализации
/// Самостоятельный интерфейс без наследования от IRepository
/// </summary>
public interface IVisualizationJobRepository
{
    #region CRUD Operations

    /// <summary>
    /// Добавить задание
    /// </summary>
    void Add(VisualizationJob entity);

    /// <summary>
    /// Обновить задание
    /// </summary>
    void Update(VisualizationJob entity);

    /// <summary>
    /// Удалить задание
    /// </summary>
    void Remove(VisualizationJob entity);

    /// <summary>
    /// Сохранить изменения
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Query Operations

    /// <summary>
    /// Получить задание по ID
    /// </summary>
    Task<VisualizationJob?> GetByIdAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить задание с изображениями
    /// </summary>
    Task<VisualizationJob?> GetByIdWithImagesAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить задания по книге
    /// </summary>
    Task<IReadOnlyList<VisualizationJob>> GetByBookIdAsync(
        Guid bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить задания по странице
    /// </summary>
    Task<IReadOnlyList<VisualizationJob>> GetByPageIdAsync(
        Guid pageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить задания пользователя
    /// </summary>
    Task<IReadOnlyList<VisualizationJob>> GetByUserIdAsync(
        Guid userId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить задания в статусе
    /// </summary>
    Task<IReadOnlyList<VisualizationJob>> GetByStatusAsync(
        VisualizationJobStatus status,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить следующее задание для обработки (из очереди)
    /// </summary>
    Task<VisualizationJob?> GetNextPendingAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование
    /// </summary>
    Task<bool> ExistsAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default);

    #endregion

    #region Queue Operations

    /// <summary>
    /// Получить количество заданий в очереди
    /// </summary>
    Task<int> GetQueueLengthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить позицию в очереди
    /// </summary>
    Task<int> GetQueuePositionAsync(
        VisualizationJobId id,
        CancellationToken cancellationToken = default);

    #endregion
}