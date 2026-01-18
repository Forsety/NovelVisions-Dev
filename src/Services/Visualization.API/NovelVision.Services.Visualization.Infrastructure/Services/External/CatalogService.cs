// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/External/CatalogService.cs

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;

namespace NovelVision.Services.Visualization.Infrastructure.Services.External;

/// <summary>
/// HTTP клиент для взаимодействия с Catalog.API
/// </summary>
public sealed class CatalogService : ICatalogService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CatalogService(
        HttpClient httpClient,
        ILogger<CatalogService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<PageContentDto>> GetPageContentAsync(
        Guid pageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting page content for PageId: {PageId}", pageId);

            var response = await _httpClient.GetAsync(
                $"/api/v1/pages/{pageId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Failed to get page content. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);

                return Result<PageContentDto>.Failure(
                    Error.NotFound($"Page {pageId} not found"));
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PageApiDto>>(
                JsonOptions, cancellationToken);

            if (apiResponse?.Data == null)
            {
                return Result<PageContentDto>.Failure(
                    Error.Failure("Invalid response from Catalog.API"));
            }

            var pageContent = new PageContentDto
            {
                Id = apiResponse.Data.Id,
                BookId = apiResponse.Data.BookId,
                ChapterId = apiResponse.Data.ChapterId,
                PageNumber = apiResponse.Data.PageNumber,
                Content = apiResponse.Data.Content,
                HasVisualization = apiResponse.Data.HasVisualization
            };

            return Result<PageContentDto>.Success(pageContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting page content for PageId: {PageId}", pageId);
            return Result<PageContentDto>.Failure(Error.Failure(ex.Message));
        }
    }

    public async Task<Result<ChapterContentDto>> GetChapterContentAsync(
        Guid chapterId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting chapter content for ChapterId: {ChapterId}", chapterId);

            var response = await _httpClient.GetAsync(
                $"/api/v1/chapters/{chapterId}/full",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<ChapterContentDto>.Failure(
                    Error.NotFound($"Chapter {chapterId} not found"));
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ChapterApiDto>>(
                JsonOptions, cancellationToken);

            if (apiResponse?.Data == null)
            {
                return Result<ChapterContentDto>.Failure(
                    Error.Failure("Invalid response from Catalog.API"));
            }

            var chapterContent = new ChapterContentDto
            {
                Id = apiResponse.Data.Id,
                BookId = apiResponse.Data.BookId,
                Title = apiResponse.Data.Title,
                ChapterNumber = apiResponse.Data.ChapterNumber,
                FullContent = apiResponse.Data.Content ?? string.Empty,
                Pages = apiResponse.Data.Pages?.Select(p => new PageContentDto
                {
                    Id = p.Id,
                    BookId = apiResponse.Data.BookId,
                    ChapterId = chapterId,
                    PageNumber = p.PageNumber,
                    Content = p.Content,
                    HasVisualization = p.HasVisualization
                }).ToList() ?? new List<PageContentDto>()
            };

            return Result<ChapterContentDto>.Success(chapterContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chapter content for ChapterId: {ChapterId}", chapterId);
            return Result<ChapterContentDto>.Failure(Error.Failure(ex.Message));
        }
    }

    public async Task<Result<BookInfoDto>> GetBookInfoAsync(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting book info for BookId: {BookId}", bookId);

            var response = await _httpClient.GetAsync(
                $"/api/v1/books/{bookId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<BookInfoDto>.Failure(
                    Error.NotFound($"Book {bookId} not found"));
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<BookApiDto>>(
                JsonOptions, cancellationToken);

            if (apiResponse?.Data == null)
            {
                return Result<BookInfoDto>.Failure(
                    Error.Failure("Invalid response from Catalog.API"));
            }

            var bookInfo = new BookInfoDto
            {
                Id = apiResponse.Data.Id,
                Title = apiResponse.Data.Title,
                AuthorId = apiResponse.Data.AuthorId,
                Genre = apiResponse.Data.Genre,
                VisualizationEnabled = apiResponse.Data.VisualizationEnabled,
                PreferredVisualizationStyle = apiResponse.Data.PreferredVisualizationStyle,
                TotalPages = apiResponse.Data.TotalPages,
                TotalChapters = apiResponse.Data.TotalChapters
            };

            return Result<BookInfoDto>.Success(bookInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book info for BookId: {BookId}", bookId);
            return Result<BookInfoDto>.Failure(Error.Failure(ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<PageInfoDto>>> GetBookPagesAsync(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting book pages for BookId: {BookId}", bookId);

            var response = await _httpClient.GetAsync(
                $"/api/v1/books/{bookId}/pages-for-visualization",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<IReadOnlyList<PageInfoDto>>.Failure(
                    Error.NotFound($"Book {bookId} not found"));
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PagesForVisualizationApiDto>>(
                JsonOptions, cancellationToken);

            if (apiResponse?.Data?.Pages == null)
            {
                return Result<IReadOnlyList<PageInfoDto>>.Failure(
                    Error.Failure("Invalid response from Catalog.API"));
            }

            var pages = apiResponse.Data.Pages.Select(p => new PageInfoDto
            {
                Id = p.PageId,
                ChapterId = p.ChapterId,
                PageNumber = p.PageNumber,
                HasVisualization = p.HasVisualization
            }).ToList();

            return Result<IReadOnlyList<PageInfoDto>>.Success(pages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book pages for BookId: {BookId}", bookId);
            return Result<IReadOnlyList<PageInfoDto>>.Failure(Error.Failure(ex.Message));
        }
    }

    public async Task<Result<bool>> UpdatePageVisualizationStatusAsync(
        Guid pageId,
        bool hasVisualization,
        string? imageUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug(
                "Updating visualization status for PageId: {PageId}, HasVisualization: {HasVisualization}",
                pageId, hasVisualization);

            var request = new
            {
                HasVisualization = hasVisualization,
                VisualizationImageUrl = imageUrl
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"/api/v1/pages/{pageId}/visualization",
                request,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Failed to update page visualization. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);

                return Result<bool>.Failure(
                    Error.Failure($"Failed to update page visualization: {response.StatusCode}"));
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating visualization status for PageId: {PageId}", pageId);
            return Result<bool>.Failure(Error.Failure(ex.Message));
        }
    }

    public async Task<Result<bool>> IsVisualizationEnabledAsync(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bookInfoResult = await GetBookInfoAsync(bookId, cancellationToken);

            if (bookInfoResult.IsFailure)
            {
                return Result<bool>.Failure(bookInfoResult.Error);
            }

            return Result<bool>.Success(bookInfoResult.Value.VisualizationEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking visualization enabled for BookId: {BookId}", bookId);
            return Result<bool>.Failure(Error.Failure(ex.Message));
        }
    }

    #region Private API DTOs

    private sealed record ApiResponse<T>
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }
    }

    private sealed record PageApiDto
    {
        public Guid Id { get; init; }
        public Guid BookId { get; init; }
        public Guid? ChapterId { get; init; }
        public int PageNumber { get; init; }
        public string Content { get; init; } = string.Empty;
        public bool HasVisualization { get; init; }
    }

    private sealed record ChapterApiDto
    {
        public Guid Id { get; init; }
        public Guid BookId { get; init; }
        public string Title { get; init; } = string.Empty;
        public int ChapterNumber { get; init; }
        public string? Content { get; init; }
        public List<PageApiDto>? Pages { get; init; }
    }

    private sealed record BookApiDto
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

    private sealed record PagesForVisualizationApiDto
    {
        public List<PageVisualizationApiDto> Pages { get; init; } = new();
    }

    private sealed record PageVisualizationApiDto
    {
        public Guid PageId { get; init; }
        public Guid ChapterId { get; init; }
        public int PageNumber { get; init; }
        public bool HasVisualization { get; init; }
    }

    #endregion
}