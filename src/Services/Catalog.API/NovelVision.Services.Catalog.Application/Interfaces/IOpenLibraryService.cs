// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Interfaces/IOpenLibraryService.cs
using System.Threading;
using System.Threading.Tasks;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Interfaces;

/// <summary>
/// Сервис для работы с Open Library API
/// </summary>
public interface IOpenLibraryService
{
    /// <summary>
    /// Получить URL обложки по ISBN
    /// </summary>
    Task<Result<string>> GetCoverByIsbnAsync(
        string isbn,
        CoverSize size = CoverSize.Medium,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить URL обложки по Open Library ID
    /// </summary>
    Task<Result<string>> GetCoverByOlidAsync(
        string olid,
        CoverSize size = CoverSize.Medium,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить метаданные книги по ISBN
    /// </summary>
    Task<Result<OpenLibraryBookDto>> GetBookByIsbnAsync(
        string isbn,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить информацию об авторе
    /// </summary>
    Task<Result<OpenLibraryAuthorDto>> GetAuthorAsync(
        string authorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Поиск книг
    /// </summary>
    Task<Result<OpenLibrarySearchResultDto>> SearchBooksAsync(
        string query,
        int page = 1,
        int limit = 20,
        CancellationToken cancellationToken = default);
}

public enum CoverSize
{
    Small,  // S
    Medium, // M
    Large   // L
}

public record OpenLibraryBookDto
{
    public string? OpenLibraryWorkId { get; init; }
    public string? OpenLibraryEditionId { get; init; }
    public string Title { get; init; } = string.Empty;
    public List<string> Authors { get; init; } = new();
    public string? Description { get; init; }
    public int? NumberOfPages { get; init; }
    public string? PublishDate { get; init; }
    public List<string> Subjects { get; init; } = new();
    public string? CoverUrl { get; init; }
}

public record OpenLibraryAuthorDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Bio { get; init; }
    public string? BirthDate { get; init; }
    public string? DeathDate { get; init; }
    public string? PhotoUrl { get; init; }
}

public record OpenLibrarySearchResultDto
{
    public int TotalResults { get; init; }
    public int Start { get; init; }
    public List<OpenLibraryBookDto> Books { get; init; } = new();
}