// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/SearchResultDto.cs
// ИСПРАВЛЕНИЕ: Единый SearchResultDto для избежания конфликтов namespace
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// Результат поиска с пагинацией и фасетами
/// </summary>
public record SearchResultDto<T>
{
    /// <summary>
    /// Найденные элементы
    /// </summary>
    public List<T> Items { get; init; } = new();

    /// <summary>
    /// Общее количество результатов
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Номер страницы
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Фасеты для фильтрации
    /// </summary>
    public SearchFacetsDto Facets { get; init; } = new();

    /// <summary>
    /// Поисковый запрос
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Время выполнения поиска
    /// </summary>
    public TimeSpan SearchDuration { get; init; }

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Фасеты для фильтрации результатов поиска
/// </summary>
public record SearchFacetsDto
{
    /// <summary>
    /// Языки
    /// </summary>
    public List<FacetItemDto> Languages { get; init; } = new();

    /// <summary>
    /// Жанры
    /// </summary>
    public List<FacetItemDto> Genres { get; init; } = new();

    /// <summary>
    /// Темы/категории
    /// </summary>
    public List<FacetItemDto> Subjects { get; init; } = new();

    /// <summary>
    /// Статусы авторских прав
    /// </summary>
    public List<FacetItemDto> CopyrightStatuses { get; init; } = new();

    /// <summary>
    /// Источники
    /// </summary>
    public List<FacetItemDto> Sources { get; init; } = new();

    /// <summary>
    /// Авторы
    /// </summary>
    public List<FacetItemDto> Authors { get; init; } = new();
}

/// <summary>
/// Элемент фасета
/// </summary>
public record FacetItemDto
{
    /// <summary>
    /// Значение
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Отображаемая метка
    /// </summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Количество результатов
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Выбран ли фасет
    /// </summary>
    public bool IsSelected { get; init; }
}