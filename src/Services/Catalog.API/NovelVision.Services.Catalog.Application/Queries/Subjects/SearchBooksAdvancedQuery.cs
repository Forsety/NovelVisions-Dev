// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/SearchBooksAdvancedQuery.cs
using System;
using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

/// <summary>
/// Расширенный поиск книг
/// </summary>
public record SearchBooksAdvancedQuery : IRequest<Result<SearchResultDto<BookListDto>>>
{
    // Текстовый поиск
    public string? SearchTerm { get; init; }
    public bool SearchInTitle { get; init; } = true;
    public bool SearchInDescription { get; init; } = true;
    public bool SearchInAuthor { get; init; } = true;

    // Фильтры
    public List<Guid> SubjectIds { get; init; } = new();
    public List<string> Genres { get; init; } = new();
    public List<string> Languages { get; init; } = new();
    public string? CopyrightStatus { get; init; }
    public string? Source { get; init; }
    public bool? IsFreeToUse { get; init; }
    public bool? IsImported { get; init; }

    // Диапазоны
    public int? MinPageCount { get; init; }
    public int? MaxPageCount { get; init; }
    public int? MinDownloadCount { get; init; }
    public DateTime? PublishedAfter { get; init; }
    public DateTime? PublishedBefore { get; init; }

    // Пагинация
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    // Сортировка
    public string SortBy { get; init; } = "relevance";
    public bool Descending { get; init; } = true;
}

/// <summary>
/// Результат поиска с фасетами
/// </summary>
public record SearchResultDto<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public SearchFacetsDto Facets { get; init; } = new();
    public string? SearchTerm { get; init; }
    public TimeSpan SearchDuration { get; init; }
}

/// <summary>
/// Фасеты для фильтрации
/// </summary>
public record SearchFacetsDto
{
    public List<FacetItemDto> Languages { get; init; } = new();
    public List<FacetItemDto> Genres { get; init; } = new();
    public List<FacetItemDto> Subjects { get; init; } = new();
    public List<FacetItemDto> CopyrightStatuses { get; init; } = new();
    public List<FacetItemDto> Sources { get; init; } = new();
}

public record FacetItemDto
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
}