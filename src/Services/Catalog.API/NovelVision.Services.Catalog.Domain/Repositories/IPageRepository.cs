// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Repositories/IPageRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Repositories;

/// <summary>
/// Репозиторий для работы со страницами
/// </summary>
public interface IPageRepository
{
    /// <summary>
    /// Получает страницу по ID
    /// </summary>
    Task<Page?> GetByIdAsync(PageId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все страницы главы
    /// </summary>
    Task<IReadOnlyList<Page>> GetByChapterIdAsync(ChapterId chapterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает страницы книги, помеченные как точки визуализации
    /// </summary>
    Task<IReadOnlyList<Page>> GetVisualizationPointsAsync(BookId bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает страницы без визуализации
    /// </summary>
    Task<IReadOnlyList<Page>> GetPagesWithoutVisualizationAsync(ChapterId chapterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает страницы с визуализацией для книги
    /// </summary>
    Task<IReadOnlyList<Page>> GetPagesWithVisualizationAsync(BookId bookId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает страницу по главе и номеру
    /// </summary>
    Task<Page?> GetByChapterAndNumberAsync(ChapterId chapterId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает количество страниц в главе
    /// </summary>
    Task<int> GetPageCountByChapterAsync(ChapterId chapterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет страницу
    /// </summary>
    Task UpdateAsync(Page page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет страницу
    /// </summary>
    Task AddAsync(Page page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет страницу
    /// </summary>
    Task DeleteAsync(Page page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет существование страницы
    /// </summary>
    Task<bool> ExistsAsync(PageId id, CancellationToken cancellationToken = default);
}