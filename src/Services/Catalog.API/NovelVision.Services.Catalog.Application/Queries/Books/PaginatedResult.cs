// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/PaginatedResult.cs
using System;
using System.Collections.Generic;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

/// <summary>
/// Пагинированный результат для запросов
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Создание результата
    /// </summary>
    public static PaginatedResult<T> Create(
        List<T> items,
        int pageNumber,
        int pageSize,
        int totalCount)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Неявное преобразование в PaginatedResultDto
    /// </summary>
    public static implicit operator PaginatedResultDto<T>(PaginatedResult<T> result)
    {
        return new PaginatedResultDto<T>
        {
            Items = result.Items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };
    }

    /// <summary>
    /// Явное преобразование из PaginatedResultDto
    /// </summary>
    public static explicit operator PaginatedResult<T>(PaginatedResultDto<T> dto)
    {
        return new PaginatedResult<T>
        {
            Items = dto.Items,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            TotalCount = dto.TotalCount,
            TotalPages = dto.TotalPages
        };
    }

    /// <summary>
    /// Преобразование в DTO
    /// </summary>
    public PaginatedResultDto<T> ToDto()
    {
        return new PaginatedResultDto<T>
        {
            Items = Items,
            PageNumber = PageNumber,
            PageSize = PageSize,
            TotalCount = TotalCount,
            TotalPages = TotalPages
        };
    }
}