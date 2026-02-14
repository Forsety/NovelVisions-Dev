// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Services/External/GutendexService.cs
// ИСПРАВЛЕНО: Парсинг форматов текста с любой кодировкой
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    // Отдельный HttpClient для загрузки текста с поддержкой редиректов
    private static readonly HttpClient TextDownloadClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    static GutendexService()
    {
        // Создаём статический HttpClient с поддержкой редиректов для загрузки текста
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        TextDownloadClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(2) // Большой таймаут для загрузки больших книг
        };
        TextDownloadClient.DefaultRequestHeaders.Add("User-Agent", "NovelVision/1.0 (Book Import Service)");
        TextDownloadClient.DefaultRequestHeaders.Add("Accept", "text/plain, text/html, */*");
    }

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

            // Логируем доступные форматы для отладки
            if (book.Formats != null)
            {
                _logger.LogInformation(
                    "Available formats for book {Id}: {Formats}",
                    gutenbergId,
                    string.Join(", ", book.Formats.Keys));
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
            // 1. Получаем метаданные книги
            var bookResult = await GetBookAsync(gutenbergId, cancellationToken);
            if (bookResult.IsFailure)
            {
                return Result<string>.Failure(bookResult.Error);
            }

            var book = bookResult.Value;

            // 2. ИСПРАВЛЕНО: Ищем URL текста более гибко
            var textUrl = FindTextUrl(book);

            if (string.IsNullOrEmpty(textUrl))
            {
                _logger.LogWarning(
                    "No text format available for book {Id}. Available formats: TextPlainUtf8={Utf8}, TextPlain={Plain}",
                    gutenbergId,
                    book.Formats?.TextPlainUtf8 ?? "null",
                    book.Formats?.TextPlain ?? "null");

                // Пробуем построить URL напрямую
                textUrl = BuildDirectTextUrl(gutenbergId);
                _logger.LogInformation("Trying direct URL: {Url}", textUrl);
            }
            else
            {
                _logger.LogInformation("Found text URL for book {Id}: {Url}", gutenbergId, textUrl);
            }

            // 3. Скачиваем текст с поддержкой редиректов
            var text = await DownloadTextWithRedirectsAsync(textUrl, cancellationToken);

            if (string.IsNullOrEmpty(text))
            {
                return Result<string>.Failure("Downloaded text is empty");
            }

            _logger.LogInformation(
                "Successfully downloaded text for book {Id}. Length: {Length} chars, ~{Words} words",
                gutenbergId,
                text.Length,
                text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length);

            return Result<string>.Success(text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting text for book {Id}", gutenbergId);
            return Result<string>.Failure($"Error getting text: {ex.Message}");
        }
    }

    /// <summary>
    /// Ищет URL текста в форматах книги
    /// </summary>
    private string? FindTextUrl(GutenbergBookDto book)
    {
        // Приоритет: UTF-8 > Plain > любой текстовый формат
        if (!string.IsNullOrEmpty(book.Formats?.TextPlainUtf8))
            return book.Formats.TextPlainUtf8;

        if (!string.IsNullOrEmpty(book.Formats?.TextPlain))
            return book.Formats.TextPlain;

        // TextUrl - computed property который проверяет оба варианта
        if (!string.IsNullOrEmpty(book.TextUrl))
            return book.TextUrl;

        return null;
    }

    /// <summary>
    /// Строит прямой URL к тексту книги на Gutenberg
    /// </summary>
    private static string BuildDirectTextUrl(int gutenbergId)
    {
        // Стандартный формат URL на Gutenberg
        return $"https://www.gutenberg.org/cache/epub/{gutenbergId}/pg{gutenbergId}.txt";
    }

    /// <summary>
    /// Скачивает текст с поддержкой редиректов (302, 301)
    /// </summary>
    private async Task<string> DownloadTextWithRedirectsAsync(
        string url,
        CancellationToken cancellationToken)
    {
        try
        {
            // Пробуем несколько альтернативных URL если основной не работает
            var urlsToTry = GetAlternativeTextUrls(url);

            foreach (var tryUrl in urlsToTry)
            {
                try
                {
                    _logger.LogDebug("Trying to download from: {Url}", tryUrl);

                    using var response = await TextDownloadClient.GetAsync(
                        tryUrl,
                        HttpCompletionOption.ResponseContentRead,
                        cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var text = await response.Content.ReadAsStringAsync(cancellationToken);
                        if (!string.IsNullOrWhiteSpace(text) && text.Length > 100)
                        {
                            _logger.LogInformation("Successfully downloaded from {Url}", tryUrl);
                            return text;
                        }
                    }
                    else
                    {
                        _logger.LogDebug("URL {Url} returned {Status}", tryUrl, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("Failed to download from {Url}: {Error}", tryUrl, ex.Message);
                }
            }

            throw new HttpRequestException($"Failed to download text from any URL. Original: {url}");
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading text from {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Генерирует альтернативные URL для загрузки текста
    /// </summary>
    private static List<string> GetAlternativeTextUrls(string originalUrl)
    {
        var urls = new List<string> { originalUrl };

        // Gutenberg использует разные домены и пути
        // Примеры:
        // https://www.gutenberg.org/files/514/514-0.txt
        // https://www.gutenberg.org/cache/epub/514/pg514.txt
        // https://gutenberg.org/ebooks/514.txt.utf-8

        if (originalUrl.Contains("gutenberg.org"))
        {
            // Извлекаем ID книги из URL
            var match = System.Text.RegularExpressions.Regex.Match(
                originalUrl,
                @"/(\d+)[-/\.]");

            if (match.Success && int.TryParse(match.Groups[1].Value, out var bookId))
            {
                // Альтернативные форматы URL
                urls.Add($"https://www.gutenberg.org/cache/epub/{bookId}/pg{bookId}.txt");
                urls.Add($"https://www.gutenberg.org/files/{bookId}/{bookId}-0.txt");
                urls.Add($"https://www.gutenberg.org/files/{bookId}/{bookId}.txt");
                urls.Add($"https://www.gutenberg.org/ebooks/{bookId}.txt.utf-8");
                urls.Add($"https://gutenberg.org/cache/epub/{bookId}/pg{bookId}.txt");
            }
        }
        else
        {
            // Если URL не содержит gutenberg.org, пробуем извлечь ID из пути
            var idMatch = System.Text.RegularExpressions.Regex.Match(originalUrl, @"(\d{2,})");
            if (idMatch.Success && int.TryParse(idMatch.Groups[1].Value, out var bookId))
            {
                urls.Add($"https://www.gutenberg.org/cache/epub/{bookId}/pg{bookId}.txt");
                urls.Add($"https://www.gutenberg.org/files/{bookId}/{bookId}-0.txt");
            }
        }

        // Убираем дубликаты
        return urls.Distinct().ToList();
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

        if (criteria.Languages?.Count > 0)
            parameters.Add($"languages={string.Join(",", criteria.Languages)}");

        if (criteria.AuthorYearStart.HasValue)
            parameters.Add($"author_year_start={criteria.AuthorYearStart}");

        if (criteria.AuthorYearEnd.HasValue)
            parameters.Add($"author_year_end={criteria.AuthorYearEnd}");

        if (!string.IsNullOrWhiteSpace(criteria.Topic))
            parameters.Add($"topic={Uri.EscapeDataString(criteria.Topic)}");

        if (criteria.Ids?.Count > 0)
            parameters.Add($"ids={string.Join(",", criteria.Ids)}");

        if (!string.IsNullOrWhiteSpace(criteria.Sort))
            parameters.Add($"sort={criteria.Sort}");

        if (criteria.Page > 1)
            parameters.Add($"page={criteria.Page}");

        return parameters.Count > 0 ? "?" + string.Join("&", parameters) : string.Empty;
    }

    private static GutenbergBookDto MapToDto(GutendexBookResponse book)
    {
        return new GutenbergBookDto
        {
            Id = book.Id,
            Title = book.Title ?? "Unknown Title",
            Authors = book.Authors?.Select(a => new GutenbergAuthorDto
            {
                Name = a.Name ?? "Unknown",
                BirthYear = a.BirthYear,
                DeathYear = a.DeathYear
            }).ToList() ?? new List<GutenbergAuthorDto>(),
            Translators = book.Translators?.Select(t => new GutenbergAuthorDto
            {
                Name = t.Name ?? "Unknown",
                BirthYear = t.BirthYear,
                DeathYear = t.DeathYear
            }).ToList() ?? new List<GutenbergAuthorDto>(),
            Subjects = book.Subjects ?? new List<string>(),
            Bookshelves = book.Bookshelves ?? new List<string>(),
            Languages = book.Languages ?? new List<string>(),
            Copyright = book.Copyright ?? false,
            MediaType = book.MediaType ?? "Text",
            Formats = MapFormats(book.Formats),
            DownloadCount = book.DownloadCount
        };
    }

    /// <summary>
    /// ИСПРАВЛЕНО: Парсит форматы с гибким поиском по ключам
    /// Gutendex возвращает ключи типа "text/plain; charset=us-ascii", "text/plain; charset=utf-8", etc.
    /// </summary>
    private static GutenbergFormatsDto MapFormats(Dictionary<string, string>? formats)
    {
        if (formats == null || formats.Count == 0)
            return new GutenbergFormatsDto();

        // Ищем текстовые форматы с любой кодировкой
        string? textPlain = null;
        string? textPlainUtf8 = null;
        string? textHtml = null;
        string? epub = null;
        string? pdf = null;
        string? imageJpeg = null;

        foreach (var (key, value) in formats)
        {
            var lowerKey = key.ToLowerInvariant();

            // Plain text - предпочитаем UTF-8
            if (lowerKey.StartsWith("text/plain"))
            {
                if (lowerKey.Contains("utf-8"))
                {
                    textPlainUtf8 = value;
                }
                else if (textPlain == null) // Берём первый попавшийся если UTF-8 ещё нет
                {
                    textPlain = value;
                }
            }
            // HTML
            else if (lowerKey.StartsWith("text/html"))
            {
                if (textHtml == null || lowerKey.Contains("utf-8"))
                {
                    textHtml = value;
                }
            }
            // EPUB
            else if (lowerKey.Contains("epub"))
            {
                epub = value;
            }
            // PDF
            else if (lowerKey.Contains("pdf"))
            {
                pdf = value;
            }
            // Image
            else if (lowerKey.StartsWith("image/jpeg") || lowerKey == "image/jpeg")
            {
                imageJpeg = value;
            }
        }

        return new GutenbergFormatsDto
        {
            TextPlain = textPlain,
            TextPlainUtf8 = textPlainUtf8,
            TextHtml = textHtml,
            ApplicationEpub = epub,
            ApplicationPdf = pdf,
            ImageJpeg = imageJpeg
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