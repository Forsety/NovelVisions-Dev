// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/SearchBooksQuery.cs
using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

public class SearchBooksQuery : IRequest<Result<PaginatedResultDto<BookListDto>>>
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Genre { get; set; }
    public string? Language { get; set; }
    public int? MinPages { get; set; }
    public int? MaxPages { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? AuthorId { get; set; }

    // New fields for Gutenberg integration
    public string? CopyrightStatus { get; set; }
    public string? Source { get; set; }
    public string? SortBy { get; set; }
    public bool Descending { get; set; } = true;
}