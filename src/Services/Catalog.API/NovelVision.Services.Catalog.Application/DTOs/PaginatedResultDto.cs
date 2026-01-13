// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/PaginatedResultDto.cs
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO для пагинированных результатов
/// </summary>
public record PaginatedResultDto<T>
{
    /// <summary>
    /// Элементы на текущей странице
    /// </summary>
    public List<T> Items { get; init; } = new();

    /// <summary>
    /// Номер текущей страницы
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Общее количество элементов
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Создание результата
    /// </summary>
    public static PaginatedResultDto<T> Create(
        List<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PaginatedResultDto<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Пустой результат
    /// </summary>
    public static PaginatedResultDto<T> Empty(int pageNumber = 1, int pageSize = 20)
    {
        return new PaginatedResultDto<T>
        {
            Items = new List<T>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = 0,
            TotalPages = 0
        };
    }
}

/// <summary>
/// DTO для результатов поиска
/// </summary>
