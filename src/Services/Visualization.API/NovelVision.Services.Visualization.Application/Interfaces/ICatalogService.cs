using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Visualization.Application.Interfaces;

/// <summary>
/// Интерфейс для взаимодействия с Catalog.API
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Получить текст страницы
    /// </summary>
    Task<Result<PageContentDto>> GetPageContentAsync(
        Guid pageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить текст главы
    /// </summary>
    Task<Result<ChapterContentDto>> GetChapterContentAsync(
        Guid chapterId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить информацию о книге
    /// </summary>
    Task<Result<BookInfoDto>> GetBookInfoAsync(
        Guid bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все страницы книги (для AutoNovel)
    /// </summary>
    Task<Result<IReadOnlyList<PageInfoDto>>> GetBookPagesAsync(
        Guid bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить флаг HasVisualization для страницы
    /// </summary>
    Task<Result<bool>> UpdatePageVisualizationStatusAsync(
        Guid pageId,
        bool hasVisualization,
        string? imageUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, поддерживает ли книга визуализацию
    /// </summary>
    Task<Result<bool>> IsVisualizationEnabledAsync(
        Guid bookId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO контента страницы
/// </summary>
public sealed record PageContentDto
{
    public Guid Id { get; init; }
    public Guid BookId { get; init; }
    public Guid? ChapterId { get; init; }
    public int PageNumber { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool HasVisualization { get; init; }
}

/// <summary>
/// DTO контента главы
/// </summary>
public sealed record ChapterContentDto
{
    public Guid Id { get; init; }
    public Guid BookId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int ChapterNumber { get; init; }
    public string FullContent { get; init; } = string.Empty;
    public IReadOnlyList<PageContentDto> Pages { get; init; } = Array.Empty<PageContentDto>();
}

/// <summary>
/// DTO информации о книге
/// </summary>
public sealed record BookInfoDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public Guid AuthorId { get; init; }
    public string? Genre { get; init; }
    public bool VisualizationEnabled { get; init; }
    public string? PreferredVisualizationStyle { get; init; }
    public int TotalPages { get; init; }
    public int TotalChapters { get; init; }
}

/// <summary>
/// DTO краткой информации о странице
/// </summary>
public sealed record PageInfoDto
{
    public Guid Id { get; init; }
    public Guid? ChapterId { get; init; }
    public int PageNumber { get; init; }
    public bool HasVisualization { get; init; }
}
