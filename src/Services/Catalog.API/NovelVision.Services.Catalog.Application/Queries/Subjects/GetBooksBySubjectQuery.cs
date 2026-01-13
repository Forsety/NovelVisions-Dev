// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Subjects/GetBooksBySubjectQuery.cs
using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Subjects;

/// <summary>
/// Запрос книг по категории
/// </summary>
public record GetBooksBySubjectQuery : IRequest<Result<PaginatedResultDto<BookListDto>>>
{
    public Guid SubjectId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public bool Descending { get; init; } = true;
    public bool IncludeChildSubjects { get; init; } = true;
}

/// <summary>
/// Запрос книг по slug категории
/// </summary>
public record GetBooksBySubjectSlugQuery : IRequest<Result<PaginatedResultDto<BookListDto>>>
{
    public string Slug { get; init; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}