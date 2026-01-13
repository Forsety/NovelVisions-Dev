// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/GetBookForReadingQuery.cs
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

            // Проверяем, опубликована ли книга
            if (!book.IsPublished)
            {
                return Result<BookForReadingDto>.Failure(
                    Error.Validation("This book is not available for reading"));
            }

            // Получаем автора
            var author = await _authorRepository.GetByIdAsync(book.AuthorId, cancellationToken);
            var authorName = author?.DisplayName ?? "Unknown Author";

            // Формируем DTO
            var dto = new BookForReadingDto
            {
                Id = book.Id.Value,
                Title = book.Metadata.Title,
                Description = book.Metadata.Description,
                AuthorName = authorName,
                AuthorId = book.AuthorId.Value,
                CoverImageUrl = book.CoverImage?.Url,
                Language = book.Metadata.Language.DisplayName,
                TotalPages = book.Chapters.Sum(c => c.Pages.Count),
                TotalChapters = book.Chapters.Count,
                WordCount = book.TotalWordCount,
                EstimatedReadingTime = book.Metadata.EstimatedReadingTime,

                // Visualization settings
                VisualizationMode = book.VisualizationMode.Name,
                AllowReaderVisualization = book.VisualizationSettings?.AllowReaderChoice ?? false,
                PreferredStyle = book.VisualizationSettings?.PreferredStyle,
                PreferredProvider = book.VisualizationSettings?.PreferredProvider,
                AllowedVisualizationModes = book.VisualizationSettings?.AllowedModes
                    .Select(m => m.Name).ToList() ?? new List<string>(),

                // Chapters
                Chapters = MapChapters(book.Chapters, request.ChapterNumber, request.IncludeChapterContent)
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

    private List<ChapterForReadingDto> MapChapters(
        IReadOnlyList<Domain.Entities.Chapter> chapters,
        int? chapterNumber,
        bool includeContent)
    {
        var orderedChapters = chapters.OrderBy(c => c.OrderIndex).ToList();

        // Если указан конкретный номер главы - фильтруем
        if (chapterNumber.HasValue)
        {
            orderedChapters = orderedChapters
                .Where(c => c.OrderIndex == chapterNumber.Value)
                .ToList();
        }

        return orderedChapters.Select((chapter, index) => new ChapterForReadingDto
        {
            Id = chapter.Id.Value,
            ChapterNumber = chapter.OrderIndex,
            Title = chapter.Title,
            Summary = chapter.Summary,
            PageCount = chapter.PageCount,
            WordCount = chapter.TotalWordCount,
            EstimatedReadingTime = chapter.EstimatedReadingTime,
            Pages = includeContent ? MapPages(chapter.Pages) : new List<PageForReadingDto>()
        }).ToList();
    }

    private List<PageForReadingDto> MapPages(IReadOnlyList<Domain.Entities.Page> pages)
    {
        return pages.OrderBy(p => p.PageNumber).Select(page => new PageForReadingDto
        {
            Id = page.Id.Value,
            PageNumber = page.PageNumber,
            Content = page.Content,
            WordCount = page.WordCount,
            HasVisualization = page.HasVisualization,
            VisualizationImageUrl = page.VisualizationImageUrl,
            VisualizationThumbnailUrl = page.VisualizationThumbnailUrl,
            IsVisualizationPoint = page.IsVisualizationPoint,
            AuthorVisualizationHint = page.AuthorVisualizationHint
        }).ToList();
    }
}