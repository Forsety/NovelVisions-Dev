// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Interfaces/IGutendexService.cs
using System.Threading;
using System.Threading.Tasks;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;

namespace NovelVision.Services.Catalog.Application.Interfaces;

/// <summary>
/// Сервис для работы с Gutendex API (Project Gutenberg)
/// </summary>
public interface IGutendexService
{
    /// <summary>
    /// Получает книгу по ID из Gutenberg
    /// </summary>
    /// <param name="gutenbergId">ID книги в Gutenberg</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Данные книги</returns>
    Task<Result<GutenbergBookDto>> GetBookAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает книгу по ID из Gutenberg (алиас для GetBookAsync)
    /// </summary>
    Task<Result<GutenbergBookDto>> GetBookByIdAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Поиск книг в Gutenberg
    /// </summary>
    /// <param name="criteria">Критерии поиска</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результаты поиска</returns>
    Task<Result<GutenbergSearchResultDto>> SearchBooksAsync(
        GutenbergSearchCriteriaDto criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Поиск книг по текстовому запросу
    /// </summary>
    /// <param name="searchQuery">Поисковый запрос</param>
    /// <param name="language">Язык (опционально)</param>
    /// <param name="page">Номер страницы</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Result<GutenbergSearchResultDto>> SearchBooksAsync(
        string searchQuery,
        string? language = null,
        int page = 1,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает популярные книги
    /// </summary>
    /// <param name="page">Номер страницы</param>
    /// <param name="pageSize">Размер страницы</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Result<GutenbergSearchResultDto>> GetPopularBooksAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает популярные книги с фильтром по языку
    /// </summary>
    /// <param name="page">Номер страницы</param>
    /// <param name="language">Язык (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Result<GutenbergSearchResultDto>> GetPopularBooksAsync(
        int page,
        string? language,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает текст книги
    /// </summary>
    /// <param name="gutenbergId">ID книги</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Result<string>> GetBookTextAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Скачивает текст книги
    /// </summary>
    Task<Result<string>> DownloadBookTextAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает URL обложки книги
    /// </summary>
    /// <param name="gutenbergId">ID книги</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<Result<string>> GetBookCoverUrlAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет доступность сервиса
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}