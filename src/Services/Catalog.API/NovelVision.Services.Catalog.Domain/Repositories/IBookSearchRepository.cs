// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Repositories/IBookSearchRepository.cs
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Repositories;

/// <summary>
/// Репозиторий для расширенного поиска книг
/// </summary>
public interface IBookSearchRepository
{
    /// <summary>
    /// Расширенный поиск книг с фильтрами
    /// </summary>
    Task<(IReadOnlyList<Book> Books, int TotalCount)> SearchAsync(
        BookSearchSpecification specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книги по тематике
    /// </summary>
    Task<IReadOnlyList<Book>> GetBySubjectAsync(
        SubjectId subjectId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить популярные книги
    /// </summary>
    Task<IReadOnlyList<Book>> GetPopularAsync(
        int count = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить недавно добавленные
    /// </summary>
    Task<IReadOnlyList<Book>> GetRecentAsync(
        int count = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книги по внешнему источнику
    /// </summary>
    Task<IReadOnlyList<Book>> GetBySourceAsync(
        BookSource source,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книгу по Gutenberg ID
    /// </summary>
    Task<Book?> GetByGutenbergIdAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить книгу по OpenLibrary ID
    /// </summary>
    Task<Book?> GetByOpenLibraryIdAsync(
        string openLibraryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование по Gutenberg ID
    /// </summary>
    Task<bool> ExistsWithGutenbergIdAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование по OpenLibrary ID
    /// </summary>
    Task<bool> ExistsWithOpenLibraryIdAsync(
        string openLibraryId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Спецификация для поиска книг
/// </summary>
public sealed record BookSearchSpecification
{
    public string? SearchTerm { get; init; }
    public List<SubjectId>? SubjectIds { get; init; }
    public List<string>? Genres { get; init; }
    public List<string>? Tags { get; init; }
    public List<string>? Languages { get; init; }
    public AuthorId? AuthorId { get; init; }
    public string? AuthorName { get; init; }
    public int? PublicationYearFrom { get; init; }
    public int? PublicationYearTo { get; init; }
    public int? AuthorBirthYearFrom { get; init; }
    public int? AuthorBirthYearTo { get; init; }
    public decimal? MinRating { get; init; }
    public bool? HasVisualization { get; init; }
    public bool? HasFullText { get; init; }
    public bool? IsPublicDomain { get; init; }
    public BookSource? Source { get; init; }
    public CopyrightStatus? CopyrightStatus { get; init; }
    public int? MinWordCount { get; init; }
    public int? MaxWordCount { get; init; }
    public BookSortOrder SortBy { get; init; } = BookSortOrder.Popularity;
    public bool SortDescending { get; init; } = true;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool IncludeAuthor { get; init; } = true;
    public bool IncludeSubjects { get; init; } = false;
    public bool IncludeChapters { get; init; } = false;
}

/// <summary>
/// Варианты сортировки книг
/// </summary>
public enum BookSortOrder
{
    Popularity,
    Rating,
    PublicationDate,
    Title,
    RecentlyAdded,
    WordCount,
    DownloadCount,
    ViewCount,
    Random
}