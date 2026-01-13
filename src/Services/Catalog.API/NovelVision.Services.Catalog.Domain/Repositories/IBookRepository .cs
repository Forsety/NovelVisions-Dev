// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Repositories/IBookRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Repositories;

/// <summary>
/// Репозиторий для работы с книгами
/// </summary>
public interface IBookRepository : IRepositoryBase<Book>
{
    /// <summary>
    /// Получить книгу по ID
    /// </summary>
    Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книгу по ISBN
    /// </summary>
    Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книги автора
    /// </summary>
    Task<IReadOnlyList<Book>> GetByAuthorAsync(AuthorId authorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование книги
    /// </summary>
    Task<bool> ExistsAsync(BookId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить уникальность ISBN
    /// </summary>
    Task<bool> IsISBNUniqueAsync(string isbn, BookId? excludeBookId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книгу по ID с главами
    /// </summary>
    Task<Book?> GetByIdWithChaptersAsync(BookId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книгу по Gutenberg ID
    /// </summary>
    Task<Book?> GetByGutenbergIdAsync(int gutenbergId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книги по категории
    /// </summary>
    Task<List<Book>> GetBySubjectAsync(SubjectId subjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить популярные книги
    /// </summary>
    Task<List<Book>> GetPopularAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книги по источнику
    /// </summary>
    Task<List<Book>> GetBySourceAsync(BookSource source, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование книги по внешнему ID
    /// </summary>
    Task<bool> ExistsByExternalIdAsync(
        ExternalSourceType sourceType,
        string externalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все книги с пагинацией
    /// </summary>
    Task<IReadOnlyList<Book>> GetAllAsync(
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Расширенный поиск книг
    /// </summary>
    Task<(List<Book> Books, int TotalCount)> SearchAdvancedAsync(
        string? searchTerm,
        List<SubjectId>? subjectIds,
        List<string>? genres,
        List<string>? languages,
        CopyrightStatus? copyrightStatus,
        BookSource? source,
        int? minPageCount,
        int? maxPageCount,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken = default);
}