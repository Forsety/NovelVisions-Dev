// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Interfaces/IBookImportService.cs
// ИСПРАВЛЕНИЕ: Добавлены отсутствующие методы
using System;
using System.Threading;
using System.Threading.Tasks;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.DTOs.Import;

namespace NovelVision.Services.Catalog.Application.Interfaces;

/// <summary>
/// Сервис импорта книг из внешних источников
/// </summary>
public interface IBookImportService
{
    #region Single Book Import

    /// <summary>
    /// Импортирует книгу по Gutenberg ID
    /// </summary>
    Task<ImportBookResultDto> ImportBookAsync(
        int gutenbergId,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Импортирует книгу по Gutenberg ID (алиас)
    /// </summary>
    Task<ImportBookResultDto> ImportGutenbergBookAsync(
        int gutenbergId,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Bulk Import

    /// <summary>
    /// Массовый импорт книг по списку ID
    /// </summary>
    Task<BulkImportResultDto> ImportBooksAsync(
        int[] gutenbergIds,
        ImportOptions? options = null,
        IProgress<BulkImportProgressDto>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Импорт популярных книг
    /// </summary>
    Task<BulkImportResultDto> ImportPopularBooksAsync(
        int count,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Импорт книг по языку
    /// </summary>
    Task<BulkImportResultDto> ImportBooksByLanguageAsync(
        string language,
        int count,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Импорт книг по теме/категории
    /// </summary>
    Task<BulkImportResultDto> ImportBooksBySubjectAsync(
        string subject,
        int count,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Sync & Status

    /// <summary>
    /// Синхронизирует книгу с внешним источником
    /// </summary>
    Task<ImportBookResultDto> SyncBookAsync(
        Guid bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, была ли книга уже импортирована
    /// </summary>
    Task<bool> IsAlreadyImportedAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает статус импорта книги
    /// </summary>
    Task<ImportStatusDto?> GetImportStatusAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Validation

    /// <summary>
    /// Проверяет доступность книги для импорта
    /// </summary>
    Task<bool> CanImportAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает информацию о книге без импорта
    /// </summary>
    Task<GutenbergBookDto?> PreviewBookAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Статус импорта книги
/// </summary>
public record ImportStatusDto
{
    /// <summary>
    /// Gutenberg ID
    /// </summary>
    public int GutenbergId { get; init; }

    /// <summary>
    /// Импортирована ли книга
    /// </summary>
    public bool IsImported { get; init; }

    /// <summary>
    /// ID книги в системе (если импортирована)
    /// </summary>
    public Guid? BookId { get; init; }

    /// <summary>
    /// Дата импорта
    /// </summary>
    public DateTime? ImportedAt { get; init; }

    /// <summary>
    /// Дата последней синхронизации
    /// </summary>
    public DateTime? LastSyncedAt { get; init; }

    /// <summary>
    /// Нужна ли синхронизация
    /// </summary>
    public bool NeedsSync { get; init; }
}