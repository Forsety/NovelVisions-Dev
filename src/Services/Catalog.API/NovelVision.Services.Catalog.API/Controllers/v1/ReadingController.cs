// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Controllers/v1/ReadingController.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.API.Models.Responses;
using NovelVision.Services.Catalog.Application.Queries.Books;

namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// REST API Controller for public reading access
/// Публичный контроллер для чтения книг без авторизации
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/reading")]
[Produces("application/json")]
[AllowAnonymous] // Публичный доступ для чтения
public class ReadingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReadingController> _logger;

    public ReadingController(
        IMediator mediator,
        ILogger<ReadingController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a published book for reading
    /// </summary>
    /// <param name="bookId">Book ID</param>
    /// <param name="chapterNumber">Optional: specific chapter number to load</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("books/{bookId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookForReadingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "chapterNumber" })] // 5 min cache
    public async Task<IActionResult> GetBookForReading(
        Guid bookId,
        [FromQuery] int? chapterNumber = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Getting book for reading: {BookId}, Chapter: {ChapterNumber}",
                bookId, chapterNumber);

            var query = new GetBookForReadingQuery
            {
                BookId = bookId,
                ChapterNumber = chapterNumber,
                IncludeChapterContent = true
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();

                if (error?.Code == "NotFound")
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = error.Message
                    });
                }

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Failed to get book",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<BookForReadingDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Book retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book for reading: {BookId}", bookId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the book"
            });
        }
    }

    /// <summary>
    /// Get a specific chapter for reading
    /// </summary>
    [HttpGet("books/{bookId:guid}/chapters/{chapterNumber:int}")]
    [ProducesResponseType(typeof(ApiResponse<ChapterForReadingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetChapterForReading(
        Guid bookId,
        int chapterNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Getting chapter for reading: BookId={BookId}, Chapter={ChapterNumber}",
                bookId, chapterNumber);

            var query = new GetBookForReadingQuery
            {
                BookId = bookId,
                ChapterNumber = chapterNumber,
                IncludeChapterContent = true
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Chapter not found"
                });
            }

            var chapter = result.Value.Chapters.FirstOrDefault();
            if (chapter == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"Chapter {chapterNumber} not found in book"
                });
            }

            return Ok(new ApiResponse<ChapterForReadingDto>
            {
                Success = true,
                Data = chapter,
                Message = "Chapter retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chapter for reading: {BookId}/{ChapterNumber}",
                bookId, chapterNumber);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the chapter"
            });
        }
    }

    /// <summary>
    /// Get book table of contents (chapters list without content)
    /// </summary>
    [HttpGet("books/{bookId:guid}/toc")]
    [ProducesResponseType(typeof(ApiResponse<BookForReadingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 600)] // 10 min cache
    public async Task<IActionResult> GetTableOfContents(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting TOC for book: {BookId}", bookId);

            var query = new GetBookForReadingQuery
            {
                BookId = bookId,
                IncludeChapterContent = false // Только метаданные глав
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Book not found"
                });
            }

            return Ok(new ApiResponse<BookForReadingDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Table of contents retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TOC for book: {BookId}", bookId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the table of contents"
            });
        }
    }

    /// <summary>
    /// Get visualization points for a book (for AuthorDefined mode)
    /// </summary>
    [HttpGet("books/{bookId:guid}/visualization-points")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationPointsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetVisualizationPoints(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting visualization points for book: {BookId}", bookId);

            var query = new GetBookForReadingQuery
            {
                BookId = bookId,
                IncludeChapterContent = true
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Book not found"
                });
            }

            // Извлекаем все точки визуализации
            var points = result.Value.Chapters
                .SelectMany(c => c.Pages
                    .Where(p => p.IsVisualizationPoint || p.HasVisualization)
                    .Select(p => new VisualizationPointDto
                    {
                        ChapterId = c.Id,
                        ChapterNumber = c.ChapterNumber,
                        ChapterTitle = c.Title,
                        PageId = p.Id,
                        PageNumber = p.PageNumber,
                        IsAuthorDefined = p.IsVisualizationPoint,
                        AuthorHint = p.AuthorVisualizationHint,
                        HasVisualization = p.HasVisualization,
                        ImageUrl = p.VisualizationImageUrl,
                        ThumbnailUrl = p.VisualizationThumbnailUrl
                    }))
                .ToList();

            var dto = new VisualizationPointsDto
            {
                BookId = bookId,
                VisualizationMode = result.Value.VisualizationMode,
                TotalPoints = points.Count,
                GeneratedCount = points.Count(p => p.HasVisualization),
                Points = points
            };

            return Ok(new ApiResponse<VisualizationPointsDto>
            {
                Success = true,
                Data = dto,
                Message = "Visualization points retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visualization points for book: {BookId}", bookId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving visualization points"
            });
        }
    }
}

/// <summary>
/// DTO для списка точек визуализации
/// </summary>
public sealed record VisualizationPointsDto
{
    public Guid BookId { get; init; }
    public string VisualizationMode { get; init; } = string.Empty;
    public int TotalPoints { get; init; }
    public int GeneratedCount { get; init; }
    public List<VisualizationPointDto> Points { get; init; } = new();
}

/// <summary>
/// DTO для точки визуализации
/// </summary>
public sealed record VisualizationPointDto
{
    public Guid ChapterId { get; init; }
    public int ChapterNumber { get; init; }
    public string ChapterTitle { get; init; } = string.Empty;
    public Guid PageId { get; init; }
    public int PageNumber { get; init; }
    public bool IsAuthorDefined { get; init; }
    public string? AuthorHint { get; init; }
    public bool HasVisualization { get; init; }
    public string? ImageUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
}