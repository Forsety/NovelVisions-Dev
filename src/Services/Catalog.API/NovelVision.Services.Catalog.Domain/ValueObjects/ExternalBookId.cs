// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/ExternalBookId.cs
using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.Enums;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Внешний идентификатор книги (Gutenberg, OpenLibrary и т.д.)
/// </summary>
public sealed class ExternalBookId : ValueObject
{
    private ExternalBookId() { }

    private ExternalBookId(
        string externalId,
        ExternalSourceType sourceType,
        string? sourceUrl,
        DateTime? lastSyncedAt,
        int? gutenbergId = null,
        string? openLibraryWorkId = null,
        string? openLibraryEditionId = null)
    {
        ExternalId = externalId;
        SourceType = sourceType;
        SourceUrl = sourceUrl;
        LastSyncedAt = lastSyncedAt;
        ImportedAt = DateTime.UtcNow;
        GutenbergId = gutenbergId;
        OpenLibraryWorkId = openLibraryWorkId;
        OpenLibraryEditionId = openLibraryEditionId;
    }

    /// <summary>
    /// Внешний ID (например, gutenberg_id = "84")
    /// </summary>
    public string ExternalId { get; private set; } = string.Empty;

    /// <summary>
    /// Тип внешнего источника
    /// </summary>
    public ExternalSourceType SourceType { get; private set; } = ExternalSourceType.Manual;

    /// <summary>
    /// URL источника
    /// </summary>
    public string? SourceUrl { get; private set; }

    /// <summary>
    /// Дата импорта
    /// </summary>
    public DateTime ImportedAt { get; private set; }

    /// <summary>
    /// Дата последней синхронизации
    /// </summary>
    public DateTime? LastSyncedAt { get; private set; }

    /// <summary>
    /// Project Gutenberg ID (числовой)
    /// </summary>
    public int? GutenbergId { get; private set; }

    /// <summary>
    /// Open Library Work ID (формат: OL12345W)
    /// </summary>
    public string? OpenLibraryWorkId { get; private set; }

    /// <summary>
    /// Open Library Edition ID (формат: OL12345M)
    /// </summary>
    public string? OpenLibraryEditionId { get; private set; }

    /// <summary>
    /// Нужна ли синхронизация (прошло более 30 дней)
    /// </summary>
    public bool NeedsSync => LastSyncedAt == null ||
                              DateTime.UtcNow - LastSyncedAt.Value > TimeSpan.FromDays(30);

    /// <summary>
    /// Есть ли Gutenberg ID
    /// </summary>
    public bool HasGutenbergId => GutenbergId.HasValue;

    /// <summary>
    /// Есть ли Open Library ID
    /// </summary>
    public bool HasOpenLibraryId => !string.IsNullOrEmpty(OpenLibraryWorkId) ||
                                     !string.IsNullOrEmpty(OpenLibraryEditionId);

    /// <summary>
    /// URL страницы на Gutenberg
    /// </summary>
    public string? GutenbergUrl => GutenbergId.HasValue
        ? $"https://www.gutenberg.org/ebooks/{GutenbergId.Value}"
        : null;

    /// <summary>
    /// URL страницы на Open Library
    /// </summary>
    public string? OpenLibraryUrl => !string.IsNullOrEmpty(OpenLibraryWorkId)
        ? $"https://openlibrary.org/works/{OpenLibraryWorkId}"
        : !string.IsNullOrEmpty(OpenLibraryEditionId)
            ? $"https://openlibrary.org/books/{OpenLibraryEditionId}"
            : null;

    #region Factory Methods

    /// <summary>
    /// Создаёт внешний ID для Gutenberg (принимает int)
    /// </summary>
    public static ExternalBookId CreateGutenberg(int gutenbergId)
    {
        Guard.Against.NegativeOrZero(gutenbergId, nameof(gutenbergId));

        return new ExternalBookId(
            gutenbergId.ToString(),
            ExternalSourceType.Gutenberg,
            $"https://www.gutenberg.org/ebooks/{gutenbergId}",
            null,
            gutenbergId: gutenbergId);
    }

    /// <summary>
    /// Создаёт внешний ID для Gutenberg (принимает string)
    /// </summary>
    public static ExternalBookId CreateGutenberg(string gutenbergIdString)
    {
        Guard.Against.NullOrWhiteSpace(gutenbergIdString, nameof(gutenbergIdString));

        if (!int.TryParse(gutenbergIdString, out var gutenbergId) || gutenbergId <= 0)
        {
            throw new ArgumentException("Invalid Gutenberg ID format", nameof(gutenbergIdString));
        }

        return CreateGutenberg(gutenbergId);
    }

    /// <summary>
    /// Создаёт внешний ID для OpenLibrary
    /// </summary>
    public static ExternalBookId CreateOpenLibrary(string workId, string? editionId = null)
    {
        Guard.Against.NullOrWhiteSpace(workId, nameof(workId));

        return new ExternalBookId(
            workId,
            ExternalSourceType.OpenLibrary,
            $"https://openlibrary.org/works/{workId}",
            null,
            openLibraryWorkId: workId,
            openLibraryEditionId: editionId);
    }

    /// <summary>
    /// Создаёт внешний ID для произвольного источника
    /// </summary>
    public static ExternalBookId Create(
        string externalId,
        ExternalSourceType sourceType,
        string? sourceUrl = null)
    {
        Guard.Against.NullOrWhiteSpace(externalId, nameof(externalId));

        return new ExternalBookId(externalId, sourceType, sourceUrl, null);
    }

    /// <summary>
    /// Пустой внешний ID
    /// </summary>
    public static ExternalBookId Empty() => new(
        string.Empty,
        ExternalSourceType.Manual,
        null,
        null);

    #endregion

    #region Modification Methods

    /// <summary>
    /// Отмечает как синхронизированный
    /// </summary>
    public void MarkSynced()
    {
        LastSyncedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновляет URL источника
    /// </summary>
    public ExternalBookId WithSourceUrl(string? url)
    {
        return new ExternalBookId(
            ExternalId,
            SourceType,
            url,
            LastSyncedAt,
            GutenbergId,
            OpenLibraryWorkId,
            OpenLibraryEditionId);
    }

    /// <summary>
    /// Добавляет Open Library IDs
    /// </summary>
    public ExternalBookId WithOpenLibrary(string? workId, string? editionId = null)
    {
        return new ExternalBookId(
            ExternalId,
            SourceType,
            SourceUrl,
            LastSyncedAt,
            GutenbergId,
            workId ?? OpenLibraryWorkId,
            editionId ?? OpenLibraryEditionId);
    }

    #endregion

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ExternalId;
        yield return SourceType;
        yield return GutenbergId;
        yield return OpenLibraryWorkId;
        yield return OpenLibraryEditionId;
    }
}