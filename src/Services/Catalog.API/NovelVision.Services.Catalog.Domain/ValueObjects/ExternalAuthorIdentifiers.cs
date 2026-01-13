// =============================================================================
// ФАЙЛ: src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/ExternalAuthorIdentifiers.cs
// ДЕЙСТВИЕ: СОЗДАТЬ новый файл
// ПРИЧИНА: Отсутствует value object для внешних идентификаторов автора
// =============================================================================

using System;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Value object для внешних идентификаторов автора
/// </summary>
public sealed record ExternalAuthorIdentifiers
{
    /// <summary>
    /// ID автора в Gutenberg
    /// </summary>
    public int? GutenbergAuthorId { get; private init; }

    /// <summary>
    /// ID автора в OpenLibrary
    /// </summary>
    public string? OpenLibraryAuthorId { get; private init; }

    /// <summary>
    /// URL страницы Wikipedia
    /// </summary>
    public string? WikipediaUrl { get; private init; }

    /// <summary>
    /// ID в Wikidata
    /// </summary>
    public string? WikidataId { get; private init; }

    /// <summary>
    /// Есть ли какие-либо внешние ID
    /// </summary>
    public bool HasAnyId => GutenbergAuthorId.HasValue ||
        !string.IsNullOrEmpty(OpenLibraryAuthorId) ||
        !string.IsNullOrEmpty(WikidataId);

    /// <summary>
    /// URL страницы в OpenLibrary
    /// </summary>
    public string? OpenLibraryUrl => !string.IsNullOrEmpty(OpenLibraryAuthorId)
        ? $"https://openlibrary.org/authors/{OpenLibraryAuthorId}"
        : null;

    /// <summary>
    /// URL страницы в Gutenberg
    /// </summary>
    public string? GutenbergUrl => GutenbergAuthorId.HasValue
        ? $"https://www.gutenberg.org/ebooks/author/{GutenbergAuthorId}"
        : null;

    private ExternalAuthorIdentifiers() { }

    /// <summary>
    /// Пустые внешние идентификаторы
    /// </summary>
    public static ExternalAuthorIdentifiers Empty => new();

    /// <summary>
    /// Создаёт идентификаторы для автора из Gutenberg
    /// </summary>
    public static ExternalAuthorIdentifiers FromGutenberg(int gutenbergId, string? wikipediaUrl = null)
    {
        return new ExternalAuthorIdentifiers
        {
            GutenbergAuthorId = gutenbergId,
            WikipediaUrl = wikipediaUrl
        };
    }

    /// <summary>
    /// Создаёт идентификаторы для автора из OpenLibrary
    /// </summary>
    public static ExternalAuthorIdentifiers FromOpenLibrary(string openLibraryId)
    {
        return new ExternalAuthorIdentifiers
        {
            OpenLibraryAuthorId = openLibraryId
        };
    }

    /// <summary>
    /// Создаёт идентификаторы с указанными значениями
    /// </summary>
    public static ExternalAuthorIdentifiers Create(
        int? gutenbergId = null,
        string? openLibraryId = null,
        string? wikipediaUrl = null,
        string? wikidataId = null)
    {
        return new ExternalAuthorIdentifiers
        {
            GutenbergAuthorId = gutenbergId,
            OpenLibraryAuthorId = openLibraryId,
            WikipediaUrl = wikipediaUrl,
            WikidataId = wikidataId
        };
    }
}