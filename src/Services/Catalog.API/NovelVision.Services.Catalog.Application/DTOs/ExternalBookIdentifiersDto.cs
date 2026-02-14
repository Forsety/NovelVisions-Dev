// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/ExternalIdentifiersDto.cs
using System;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO для внешних идентификаторов книги
/// </summary>
public record ExternalBookIdentifiersDto
{
    public int? GutenbergId { get; init; }
    public string? OpenLibraryWorkId { get; init; }
    public string? OpenLibraryEditionId { get; init; }
    public string? GoogleBooksId { get; init; }
    public string? LibraryThingId { get; init; }
    public string? GoodreadsId { get; init; }

    // Computed URLs
    public string? GutenbergUrl { get; init; }
    public string? OpenLibraryUrl { get; init; }

    public bool HasAnyId => GutenbergId.HasValue ||
        !string.IsNullOrEmpty(OpenLibraryWorkId) ||
        !string.IsNullOrEmpty(GoogleBooksId);
}

/// <summary>
/// DTO для внешних идентификаторов автора
/// </summary>
public record ExternalAuthorIdentifiersDto
{
    public int? GutenbergAuthorId { get; init; }
    public string? OpenLibraryAuthorId { get; init; }
    public string? WikipediaUrl { get; init; }
    public string? WikidataId { get; init; }

    // Computed URL
    public string? OpenLibraryUrl { get; init; }

    public bool HasAnyId => GutenbergAuthorId.HasValue ||
        !string.IsNullOrEmpty(OpenLibraryAuthorId);
}