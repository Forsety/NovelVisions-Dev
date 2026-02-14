// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/GetBookForReadingQuery.cs
// FIXED: Allows reading of Public Domain / Gutenberg books even if not "Published"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Enums;  // ADDED for BookSource
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

/// <summary>
/// Запрос на получение книги для чтения (публичный, без авторизации)
/// </summary>
public sealed record GetBookForReadingQuery : IRequest<Result<BookForReadingDto>>
{
    /// <summary>
    /// ID книги
    /// </summary>
    public Guid BookId { get; init; }

    /// <summary>
    /// Включить содержимое глав
    /// </summary>
    public bool IncludeChapterContent { get; init; } = true;

    /// <summary>
    /// Номер главы для загрузки (null = все главы)
    /// </summary>
    public int? ChapterNumber { get; init; }
}

/// <summary>
/// DTO для чтения книги
/// </summary>
public sealed record BookForReadingDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public Guid AuthorId { get; init; }
    public string? CoverImageUrl { get; init; }
    public string Language { get; init; } = "en";
    public int TotalPages { get; init; }
    public int TotalChapters { get; init; }
    public int WordCount { get; init; }
    public TimeSpan EstimatedReadingTime { get; init; }

    // Visualization settings
    public string VisualizationMode { get; init; } = "None";
    public bool AllowReaderVisualization { get; init; }
    public string? PreferredStyle { get; init; }
    public string? PreferredProvider { get; init; }
    public List<string> AllowedVisualizationModes { get; init; } = new();

    // Content
    public List<ChapterForReadingDto> Chapters { get; init; } = new();
}

/// <summary>
/// DTO главы для чтения
/// </summary>
public sealed record ChapterForReadingDto
{
    public Guid Id { get; init; }
    public int ChapterNumber { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public int PageCount { get; init; }
    public int WordCount { get; init; }
    public TimeSpan EstimatedReadingTime { get; init; }
    public List<PageForReadingDto> Pages { get; init; } = new();
}

/// <summary>
/// DTO страницы для чтения
/// </summary>
public sealed record PageForReadingDto
{
    public Guid Id { get; init; }
    public int PageNumber { get; init; }
    public string Content { get; init; } = string.Empty;
    public int WordCount { get; init; }

    // Visualization
    public bool HasVisualization { get; init; }
    public string? VisualizationImageUrl { get; init; }
    public string? VisualizationThumbnailUrl { get; init; }
    public bool IsVisualizationPoint { get; init; }
    public string? AuthorVisualizationHint { get; init; }
}

/// <summary>
/// Handler для GetBookForReadingQuery
/// </summary>
public sealed class GetBookForReadingQueryHandler
    : IRequestHandler<GetBookForReadingQuery, Result<BookForReadingDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ILogger<GetBookForReadingQueryHandler> _logger;

    public GetBookForReadingQueryHandler(
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        ILogger<GetBookForReadingQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _logger = logger;
    }

    public async Task<Result<BookForReadingDto>> Handle(
        GetBookForReadingQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting book for reading: {BookId}", request.BookId);

            var bookId = BookId.From(request.BookId);
            var book = await _bookRepository.GetByIdWithChaptersAsync(bookId, cancellationToken);

            if (book == null)
            {
                return Result<BookForReadingDto>.Failure(
                    Error.NotFound($"Book with ID {request.BookId} not found"));
            }

            // FIXED: Check if book can be read
            // Books can be read if:
            // 1. Published
            // 2. Public Domain (copyright status)
            // 3. From Gutenberg source with content
            // 4. Has chapters and pages (content exists)
            var isGutenbergSource = book.Source?.Name == "Gutenberg" ||
                                    book.Source == BookSource.Gutenberg;
            var hasContent = book.ChapterCount > 0 || book.TotalPageCount > 0;

            var canRead = book.IsPublished ||
                          book.IsPublicDomain ||
                          (isGutenbergSource && hasContent) ||
                          (book.ChapterCount > 0 && book.TotalPageCount > 0);

            if (!canRead)
            {
                return Result<BookForReadingDto>.Failure(
                    Error.Validation("This book is not available for reading yet"));
            }

            // Получаем автора
            var author = await _authorRepository.GetByIdAsync(book.AuthorId, cancellationToken);
            var authorName = author?.DisplayName ?? "Unknown Author";

            // FIXED: Null-safe access for all nullable properties
            var chapters = book.Chapters ?? new List<Domain.Entities.Chapter>();
            var totalPages = chapters.Sum(c => c.Pages?.Count ?? 0);
            var totalWordCount = book.TotalWordCount > 0
                ? book.TotalWordCount
                : chapters.Sum(c => c.TotalWordCount);

            // Формируем DTO
            var dto = new BookForReadingDto
            {
                Id = book.Id.Value,
                Title = book.Metadata?.Title ?? "Untitled",
                Description = book.Metadata?.Description,
                AuthorName = authorName,
                AuthorId = book.AuthorId.Value,
                CoverImageUrl = book.CoverImage?.Url,
                Language = book.Metadata?.Language ?? "en",
                TotalPages = totalPages,
                TotalChapters = chapters.Count,
                WordCount = totalWordCount,
                EstimatedReadingTime = TimeSpan.FromMinutes(totalWordCount / 200.0),

                // FIXED: Null-safe visualization settings
                VisualizationMode = book.VisualizationMode?.Name ?? "None",
                AllowReaderVisualization = book.VisualizationSettings?.AllowReaderChoice ?? false,
                PreferredStyle = book.VisualizationSettings?.PreferredStyle,
                PreferredProvider = book.VisualizationSettings?.PreferredProvider,
                AllowedVisualizationModes = book.VisualizationSettings?.AllowedModes?
                    .Select(m => m.Name).ToList() ?? new List<string>(),

                // Chapters
                Chapters = MapChapters(chapters, request.ChapterNumber, request.IncludeChapterContent)
            };

            return Result<BookForReadingDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book for reading: {BookId}", request.BookId);
            return Result<BookForReadingDto>.Failure(
                Error.Failure($"Failed to get book: {ex.Message}"));
        }
    }

    private static List<ChapterForReadingDto> MapChapters(
        IReadOnlyList<Domain.Entities.Chapter> chapters,
        int? chapterNumber,
        bool includeContent)
    {
        if (chapters == null || chapters.Count == 0)
            return new List<ChapterForReadingDto>();

        var orderedChapters = chapters.OrderBy(c => c.OrderIndex).ToList();

        // Если указан конкретный номер главы - фильтруем
        if (chapterNumber.HasValue)
        {
            orderedChapters = orderedChapters
                .Where(c => c.OrderIndex == chapterNumber.Value)
                .ToList();
        }

        return orderedChapters.Select(chapter => new ChapterForReadingDto
        {
            Id = chapter.Id.Value,
            ChapterNumber = chapter.OrderIndex,
            Title = chapter.Title ?? $"Chapter {chapter.OrderIndex}",
            Summary = chapter.Summary,
            PageCount = chapter.PageCount,
            WordCount = chapter.TotalWordCount,
            EstimatedReadingTime = chapter.EstimatedReadingTime,
            Pages = includeContent ? MapPages(chapter.Pages) : new List<PageForReadingDto>()
        }).ToList();
    }

    private static List<PageForReadingDto> MapPages(IReadOnlyList<Domain.Entities.Page>? pages)
    {
        if (pages == null || pages.Count == 0)
            return new List<PageForReadingDto>();

        return pages.OrderBy(p => p.PageNumber).Select(page => new PageForReadingDto
        {
            Id = page.Id.Value,
            PageNumber = page.PageNumber,
            Content = page.Content ?? string.Empty,
            WordCount = page.WordCount,
            HasVisualization = page.HasVisualization,
            VisualizationImageUrl = page.VisualizationImageUrl,
            VisualizationThumbnailUrl = page.VisualizationThumbnailUrl,
            IsVisualizationPoint = page.IsVisualizationPoint,
            AuthorVisualizationHint = page.AuthorVisualizationHint
        }).ToList();
    }
}