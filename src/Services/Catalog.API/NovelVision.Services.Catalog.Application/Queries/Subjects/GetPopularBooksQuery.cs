// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/GetPopularBooksQuery.cs
using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

/// <summary>
/// Запрос популярных книг
/// </summary>
public record GetPopularBooksQuery : IRequest<Result<List<BookListDto>>>
{
    public int Count { get; init; } = 20;
    public string? Language { get; init; }
    public bool OnlyFreeToUse { get; init; } = false;
    public List<Guid> SubjectIds { get; init; } = new();
}

/// <summary>
/// Запрос недавно добавленных книг
/// </summary>
public record GetRecentBooksQuery : IRequest<Result<List<BookListDto>>>
{
    public int Count { get; init; } = 20;
    public string? Language { get; init; }
    public bool OnlyImported { get; init; } = false;
}

/// <summary>
/// Запрос книг по источнику
/// </summary>
public record GetBooksBySourceQuery : IRequest<Result<PaginatedResultDto<BookListDto>>>
{
    public string Source { get; init; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}