// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Services/External/GutendexService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;
using NovelVision.Services.Catalog.Application.Interfaces;

namespace NovelVision.Services.Catalog.Infrastructure.Services.External;

/// <summary>
/// Реализация сервиса Gutendex API
/// </summary>
public class GutendexService : IGutendexService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GutendexService> _logger;
    private const string BaseUrl = "https://gutendex.com";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public GutendexService(HttpClient httpClient, ILogger<GutendexService> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _logger = logger;
    }

    #region Get Book

    public async Task<Result<GutenbergBookDto>> GetBookAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching Gutenberg book {Id}", gutenbergId);

            var response = await _httpClient.GetAsync($"/books/{gutenbergId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                if (statusCode == 404)
                {
                    return Result<GutenbergBookDto>.Failure(
                        Error.NotFound($"Book with ID {gutenbergId} not found in Gutenberg"));
                }
                return Result<GutenbergBookDto>.Failure(
                    $"Gutendex API returned {response.StatusCode}");
            }

            var book = await response.Content.ReadFromJsonAsync<GutendexBookResponse>(
                JsonOptions, cancellationToken);

            if (book is null)
            {
                return Result<GutenbergBookDto>.Failure("Failed to parse Gutendex response");
            }

            return Result<GutenbergBookDto>.Success(MapToDto(book));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching Gutenberg book {Id}", gutenbergId);
            return Result<GutenbergBookDto>.Failure($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout fetching Gutenberg book {Id}", gutenbergId);
            return Result<GutenbergBookDto>.Failure("Request timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Gutenberg book {Id}", gutenbergId);
            return Result<GutenbergBookDto>.Failure($"Error fetching book: {ex.Message}");
        }
    }

    public Task<Result<GutenbergBookDto>> GetBookByIdAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        return GetBookAsync(gutenbergId, cancellationToken);
    }

    #endregion

    #region Search

    public async Task<Result<GutenbergSearchResultDto>> SearchBooksAsync(
        GutenbergSearchCriteriaDto criteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = BuildQueryString(criteria);
            var url = $"/books{queryParams}";

            _logger.LogInformation("Searching Gutendex: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result<GutenbergSearchResultDto>.Failure(
                    $"Gutendex API returned {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<GutendexSearchResponse>(
                JsonOptions, cancellationToken);

            if (result is null)
            {
                return Result<GutenbergSearchResultDto>.Failure("Failed to parse Gutendex response");
            }

            return Result<GutenbergSearchResultDto>.Success(new GutenbergSearchResultDto
            {
                Count = result.Count,
                Next = result.Next,
                Previous = result.Previous,
                Results = result.Results.Select(MapToDto).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Gutendex");
            return Result<GutenbergSearchResultDto>.Failure($"Error searching: {ex.Message}");
        }
    }

    public async Task<Result<GutenbergSearchResultDto>> SearchBooksAsync(
        string searchQuery,
        string? language = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var criteria = new GutenbergSearchCriteriaDto
        {
            Search = searchQuery,
            Languages = language != null ? new List<string> { language } : null,
            Page = page
        };

        return await SearchBooksAsync(criteria, cancellationToken);
    }

    #endregion

    #region Popular Books

    public async Task<Result<GutenbergSearchResultDto>> GetPopularBooksAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var criteria = new GutenbergSearchCriteriaDto
        {
            Page = page,
            Sort = "popular"
        };

        return await SearchBooksAsync(criteria, cancellationToken);
    }

    public async Task<Result<GutenbergSearchResultDto>> GetPopularBooksAsync(
        int page,
        string? language,
        CancellationToken cancellationToken = default)
    {
        var criteria = new GutenbergSearchCriteriaDto
        {
            Page = page,
            Languages = language != null ? new List<string> { language } : null,
            Sort = "popular"
        };

        return await SearchBooksAsync(criteria, cancellationToken);
    }

    #endregion

    #region Text & Cover

    public async Task<Result<string>> GetBookTextAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bookResult = await GetBookAsync(gutenbergId, cancellationToken);
            if (bookResult.IsFailure)
            {
                return Result<string>.Failure(bookResult.Error);
            }

            var textUrl = bookResult.Value.Formats?.TextPlainUtf8
                       ?? bookResult.Value.Formats?.TextPlain;

            if (string.IsNullOrEmpty(textUrl))
            {
                return Result<string>.Failure("No text format available for this book");
            }

            _logger.LogInformation("Downloading text from {Url}", textUrl);

            var text = await _httpClient.GetStringAsync(textUrl, cancellationToken);
            return Result<string>.Success(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting text for book {Id}", gutenbergId);
            return Result<string>.Failure($"Error getting text: {ex.Message}");
        }
    }

    public Task<Result<string>> DownloadBookTextAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        return GetBookTextAsync(gutenbergId, cancellationToken);
    }

    public async Task<Result<string>> GetBookCoverUrlAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        var bookResult = await GetBookAsync(gutenbergId, cancellationToken);
        if (bookResult.IsFailure)
        {
            return Result<string>.Failure(bookResult.Error);
        }

        var coverUrl = bookResult.Value.CoverImageUrl;
        if (string.IsNullOrEmpty(coverUrl))
        {
            return Result<string>.Failure("No cover image available");
        }

        return Result<string>.Success(coverUrl);
    }

    #endregion

    #region Availability

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/books?page=1", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Private Methods

    private static string BuildQueryString(GutenbergSearchCriteriaDto criteria)
    {
        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(criteria.Search))
            parameters.Add($"search={Uri.EscapeDataString(criteria.Search)}");

        if (criteria.Languages?.Any() == true)
            parameters.Add($"languages={string.Join(",", criteria.Languages)}");

        if (!string.IsNullOrWhiteSpace(criteria.Topic))
            parameters.Add($"topic={Uri.EscapeDataString(criteria.Topic)}");

        if (criteria.AuthorYearStart.HasValue)
            parameters.Add($"author_year_start={criteria.AuthorYearStart}");

        if (criteria.AuthorYearEnd.HasValue)
            parameters.Add($"author_year_end={criteria.AuthorYearEnd}");

        if (criteria.Copyright.HasValue)
            parameters.Add($"copyright={criteria.Copyright.Value.ToString().ToLower()}");

        if (criteria.Ids?.Any() == true)
            parameters.Add($"ids={string.Join(",", criteria.Ids)}");

        if (!string.IsNullOrWhiteSpace(criteria.Sort))
            parameters.Add($"sort={criteria.Sort}");

        if (criteria.Page > 1)
            parameters.Add($"page={criteria.Page}");

        return parameters.Any() ? "?" + string.Join("&", parameters) : "";
    }

    private static GutenbergBookDto MapToDto(GutendexBookResponse response)
    {
        return new GutenbergBookDto
        {
            Id = response.Id,
            Title = response.Title ?? "Unknown Title",
            Authors = response.Authors?.Select(a => new GutenbergAuthorDto
            {
                Name = a.Name ?? "Unknown",
                BirthYear = a.BirthYear,
                DeathYear = a.DeathYear
            }).ToList() ?? new List<GutenbergAuthorDto>(),
            Translators = response.Translators?.Select(t => new GutenbergAuthorDto
            {
                Name = t.Name ?? "Unknown",
                BirthYear = t.BirthYear,
                DeathYear = t.DeathYear
            }).ToList() ?? new List<GutenbergAuthorDto>(),
            Subjects = response.Subjects ?? new List<string>(),
            Bookshelves = response.Bookshelves ?? new List<string>(),
            Languages = response.Languages ?? new List<string>(),
            Copyright = response.Copyright,
            MediaType = response.MediaType ?? "Text",
            Formats = MapFormats(response.Formats),
            DownloadCount = response.DownloadCount
        };
    }

    private static GutenbergFormatsDto MapFormats(Dictionary<string, string>? formats)
    {
        if (formats == null) return new GutenbergFormatsDto();

        return new GutenbergFormatsDto
        {
            TextPlain = formats.TryGetValue("text/plain", out var tp) ? tp : null,
            TextPlainUtf8 = formats.TryGetValue("text/plain; charset=utf-8", out var tpu) ? tpu : null,
            TextHtml = formats.TryGetValue("text/html", out var th) ? th : null,
            ApplicationEpub = formats.TryGetValue("application/epub+zip", out var epub) ? epub : null,
            ApplicationPdf = formats.TryGetValue("application/pdf", out var pdf) ? pdf : null,
            ImageJpeg = formats.TryGetValue("image/jpeg", out var img) ? img : null
        };
    }

    #endregion

    #region Internal Response Classes

    private class GutendexSearchResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<GutendexBookResponse> Results { get; set; } = new();
    }

    private class GutendexBookResponse
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public List<GutendexAuthorResponse>? Authors { get; set; }
        public List<GutendexAuthorResponse>? Translators { get; set; }
        public List<string>? Subjects { get; set; }
        public List<string>? Bookshelves { get; set; }
        public List<string>? Languages { get; set; }
        public bool? Copyright { get; set; }
        public string? MediaType { get; set; }
        public Dictionary<string, string>? Formats { get; set; }
        public int DownloadCount { get; set; }
    }

    private class GutendexAuthorResponse
    {
        public string? Name { get; set; }
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
    }

    #endregion
}