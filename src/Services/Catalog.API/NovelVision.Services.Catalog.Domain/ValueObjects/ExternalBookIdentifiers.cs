// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/ExternalBookIdentifiers.cs
using System;
using System.Collections.Generic;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Идентификаторы книги во внешних системах
/// </summary>
public sealed class ExternalBookIdentifiers : ValueObject
{
    private ExternalBookIdentifiers(
        int? gutenbergId,
        string? openLibraryWorkId,
        string? openLibraryEditionId,
        string? googleBooksId,
        string? libraryThingId,
        string? goodreadsId)
    {
        GutenbergId = gutenbergId;
        OpenLibraryWorkId = openLibraryWorkId;
        OpenLibraryEditionId = openLibraryEditionId;
        GoogleBooksId = googleBooksId;
        LibraryThingId = libraryThingId;
        GoodreadsId = goodreadsId;
    }

    /// <summary>
    /// Project Gutenberg ID
    /// </summary>
    public int? GutenbergId { get; }

    /// <summary>
    /// Open Library Work ID (формат: OL12345W)
    /// </summary>
    public string? OpenLibraryWorkId { get; }

    /// <summary>
    /// Open Library Edition ID (формат: OL12345M)
    /// </summary>
    public string? OpenLibraryEditionId { get; }

    /// <summary>
    /// Google Books ID
    /// </summary>
    public string? GoogleBooksId { get; }

    /// <summary>
    /// LibraryThing ID
    /// </summary>
    public string? LibraryThingId { get; }

    /// <summary>
    /// Goodreads ID
    /// </summary>
    public string? GoodreadsId { get; }

    /// <summary>
    /// Есть ли хотя бы один внешний идентификатор
    /// </summary>
    public bool HasAnyId =>
        GutenbergId.HasValue ||
        !string.IsNullOrEmpty(OpenLibraryWorkId) ||
        !string.IsNullOrEmpty(OpenLibraryEditionId) ||
        !string.IsNullOrEmpty(GoogleBooksId) ||
        !string.IsNullOrEmpty(LibraryThingId) ||
        !string.IsNullOrEmpty(GoodreadsId);

    /// <summary>
    /// Есть ли Gutenberg ID (для синхронизации)
    /// </summary>
    public bool HasGutenbergId => GutenbergId.HasValue;

    /// <summary>
    /// Есть ли Open Library ID (для обложек и метаданных)
    /// </summary>
    public bool HasOpenLibraryId =>
        !string.IsNullOrEmpty(OpenLibraryWorkId) ||
        !string.IsNullOrEmpty(OpenLibraryEditionId);

    /// <summary>
    /// Пустой объект (без идентификаторов)
    /// </summary>
    public static ExternalBookIdentifiers Empty() => new(null, null, null, null, null, null);

    /// <summary>
    /// Создание из Gutenberg ID
    /// </summary>
    public static ExternalBookIdentifiers FromGutenberg(int gutenbergId)
    {
        if (gutenbergId <= 0)
            throw new ArgumentException("Gutenberg ID must be positive", nameof(gutenbergId));

        return new ExternalBookIdentifiers(gutenbergId, null, null, null, null, null);
    }

    /// <summary>
    /// Создание из Open Library IDs
    /// </summary>
    public static ExternalBookIdentifiers FromOpenLibrary(string? workId, string? editionId = null)
    {
        return new ExternalBookIdentifiers(null, workId, editionId, null, null, null);
    }

    /// <summary>
    /// Полное создание
    /// </summary>
    public static ExternalBookIdentifiers Create(
        int? gutenbergId = null,
        string? openLibraryWorkId = null,
        string? openLibraryEditionId = null,
        string? googleBooksId = null,
        string? libraryThingId = null,
        string? goodreadsId = null)
    {
        return new ExternalBookIdentifiers(
            gutenbergId,
            openLibraryWorkId,
            openLibraryEditionId,
            googleBooksId,
            libraryThingId,
            goodreadsId);
    }

    /// <summary>
    /// Обновление Gutenberg ID
    /// </summary>
    public ExternalBookIdentifiers WithGutenbergId(int gutenbergId)
    {
        return new ExternalBookIdentifiers(
            gutenbergId,
            OpenLibraryWorkId,
            OpenLibraryEditionId,
            GoogleBooksId,
            LibraryThingId,
            GoodreadsId);
    }

    /// <summary>
    /// Обновление Open Library IDs
    /// </summary>
    public ExternalBookIdentifiers WithOpenLibrary(string? workId, string? editionId = null)
    {
        return new ExternalBookIdentifiers(
            GutenbergId,
            workId ?? OpenLibraryWorkId,
            editionId ?? OpenLibraryEditionId,
            GoogleBooksId,
            LibraryThingId,
            GoodreadsId);
    }

    /// <summary>
    /// URL на Gutenberg страницу книги
    /// </summary>
    public string? GutenbergUrl => GutenbergId.HasValue
        ? $"https://www.gutenberg.org/ebooks/{GutenbergId.Value}"
        : null;

    /// <summary>
    /// URL на Open Library страницу
    /// </summary>
    public string? OpenLibraryUrl => !string.IsNullOrEmpty(OpenLibraryWorkId)
        ? $"https://openlibrary.org/works/{OpenLibraryWorkId}"
        : !string.IsNullOrEmpty(OpenLibraryEditionId)
            ? $"https://openlibrary.org/books/{OpenLibraryEditionId}"
            : null;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GutenbergId;
        yield return OpenLibraryWorkId;
        yield return OpenLibraryEditionId;
        yield return GoogleBooksId;
        yield return LibraryThingId;
        yield return GoodreadsId;
    }
}