using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.API.Models.Responses;
using NovelVision.Services.Catalog.Application.Commands.Books;
using NovelVision.Services.Catalog.Application.Commands.Pages;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.Queries.Books;
using NovelVision.Services.Catalog.Application.Queries.Pages;

namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// Book visualization endpoints: settings + visualization plan + page visualization operations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Produces("application/json")]
[Authorize(Policy = "RequireAuthorRole")]
public sealed class BooksVisualizationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<BooksVisualizationController> _logger;

    public BooksVisualizationController(
        IMediator mediator,
        ICurrentUserService currentUser,
        ILogger<BooksVisualizationController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get visualization settings for a book.
    /// </summary>
    [HttpGet("books/{bookId:guid}/visualization/settings")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVisualizationSettings(
        Guid bookId,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting visualization settings for book {BookId}", bookId);

        if (!await CanEditBook(bookId, ct))
        {
            return ForbidResponse("You don't have permission to view visualization settings for this book");
        }

        var result = await _mediator.Send(new GetBookVisualizationSettingsQuery { BookId = bookId }, ct);
        return ToApiResponse(result, "Visualization settings retrieved successfully");
    }

    /// <summary>
    /// Update visualization settings for a book.
    /// </summary>
    [HttpPut("books/{bookId:guid}/visualization/settings")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisualizationSettings(
        Guid bookId,
        [FromBody] SetBookVisualizationSettingsRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Updating visualization settings for book {BookId}", bookId);

        if (!await CanEditBook(bookId, ct))
        {
            return ForbidResponse("You don't have permission to update visualization settings for this book");
        }

        var command = new SetVisualizationSettingsCommand
        {
            BookId = bookId,
            Mode = request.Mode,
            AllowReaderChoice = request.AllowReaderChoice,
            AllowedModes = request.AllowedModes,
            Style = request.Style,
            Provider = request.Provider,
            MaxImagesPerPage = request.MaxImagesPerPage,
            AutoGenerateOnPublish = request.AutoGenerateOnPublish
        };

        var result = await _mediator.Send(command, ct);
        return ToApiResponse(result, "Visualization settings updated successfully");
    }

    /// <summary>
    /// Get pages plan for visualization (filterable).
    /// </summary>
    [HttpGet("books/{bookId:guid}/visualization/pages")]
    [ProducesResponseType(typeof(ApiResponse<PagesForVisualizationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPagesForVisualization(
        Guid bookId,
        [FromQuery] Guid? chapterId = null,
        [FromQuery] bool onlyWithoutVisualization = false,
        [FromQuery] bool onlyVisualizationPoints = false,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Getting pages for visualization. BookId={BookId}, ChapterId={ChapterId}, OnlyWithout={OnlyWithout}, OnlyPoints={OnlyPoints}",
            bookId, chapterId, onlyWithoutVisualization, onlyVisualizationPoints);

        if (!await CanEditBook(bookId, ct))
        {
            return ForbidResponse("You don't have permission to view visualization plan for this book");
        }

        var query = new GetPagesForVisualizationQuery
        {
            BookId = bookId,
            ChapterId = chapterId,
            OnlyWithoutVisualization = onlyWithoutVisualization,
            OnlyVisualizationPoints = onlyVisualizationPoints
        };

        var result = await _mediator.Send(query, ct);
        return ToApiResponse(result, "Visualization pages retrieved successfully");
    }

    /// <summary>
    /// Mark/unmark a page as a visualization point (AuthorDefined mode support).
    /// </summary>
    [HttpPut("books/{bookId:guid}/chapters/{chapterId:guid}/pages/{pageId:guid}/visualization-point")]
    [ProducesResponseType(typeof(ApiResponse<PageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetVisualizationPoint(
        Guid bookId,
        Guid chapterId,
        Guid pageId,
        [FromBody] SetVisualizationPointRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Setting visualization point. BookId={BookId}, ChapterId={ChapterId}, PageId={PageId}, IsPoint={IsPoint}",
            bookId, chapterId, pageId, request.IsVisualizationPoint);

        if (!await CanEditBook(bookId, ct))
        {
            return ForbidResponse("You don't have permission to modify visualization markers for this book");
        }

        var command = new MarkVisualizationPointCommand
        {
            BookId = bookId,
            ChapterId = chapterId,
            PageId = pageId,
            IsVisualizationPoint = request.IsVisualizationPoint,
            AuthorHint = request.AuthorHint
        };

        var result = await _mediator.Send(command, ct);
        return ToApiResponse(result, "Visualization point updated successfully");
    }

    /// <summary>
    /// Persist visualization result (URLs/jobId) for a page.
    /// Typically called by Visualization.API when job is completed.
    /// </summary>
    [HttpPut("books/{bookId:guid}/chapters/{chapterId:guid}/pages/{pageId:guid}/visualization")]
    [ProducesResponseType(typeof(ApiResponse<PageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPageVisualization(
        Guid bookId,
        Guid chapterId,
        Guid pageId,
        [FromBody] SetPageVisualizationRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Setting page visualization. BookId={BookId}, ChapterId={ChapterId}, PageId={PageId}, JobId={JobId}",
            bookId, chapterId, pageId, request.VisualizationJobId);

        if (!await CanEditBook(bookId, ct))
        {
            return ForbidResponse("You don't have permission to set visualization data for this book");
        }

        var command = new SetPageVisualizationCommand
        {
            BookId = bookId,
            ChapterId = chapterId,
            PageId = pageId,
            ImageUrl = request.ImageUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            VisualizationJobId = request.VisualizationJobId
        };

        var result = await _mediator.Send(command, ct);
        return ToApiResponse(result, "Page visualization set successfully");
    }

    /// <summary>
    /// Clear visualization for a page (remove URLs/job link).
    /// </summary>
    [HttpDelete("books/{bookId:guid}/chapters/{chapterId:guid}/pages/{pageId:guid}/visualization")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearPageVisualization(
        Guid bookId,
        Guid chapterId,
        Guid pageId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Clearing page visualization. BookId={BookId}, ChapterId={ChapterId}, PageId={PageId}",
            bookId, chapterId, pageId);

        if (!await CanEditBook(bookId, ct))
        {
            return ForbidResponse("You don't have permission to clear visualization data for this book");
        }

        var command = new ClearPageVisualizationCommand
        {
            BookId = bookId,
            ChapterId = chapterId,
            PageId = pageId
        };

        var result = await _mediator.Send(command, ct);
        return ToApiResponse(result, "Page visualization cleared successfully");
    }

    // -------------------------
    // Helpers
    // -------------------------

    private IActionResult ToApiResponse<T>(Result<T> result, string successMessage)
    {
        if (result.IsSucceeded)
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Data = result.Value,
                Message = successMessage
            });
        }

        var error = result.Error;
        var message = error?.Message ?? "Request failed";

        if (error != null && error.IsType(ErrorType.NotFound))
        {
            return NotFound(new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = result.Errors.Select(e => e.Message).ToList()
            });
        }

        if (error != null && error.IsType(ErrorType.Conflict))
        {
            return StatusCode(StatusCodes.Status409Conflict, new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = result.Errors.Select(e => e.Message).ToList()
            });
        }

        if (error != null && (error.IsType(ErrorType.Forbidden) || error.IsType(ErrorType.Unauthorized)))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = result.Errors.Select(e => e.Message).ToList()
            });
        }

        // Validation + fallback
        return BadRequest(new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = result.Errors.Select(e => e.Message).ToList()
        });
    }

    private IActionResult ForbidResponse(string message)
        => StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
        {
            Success = false,
            Message = message
        });

    private async Task<Guid?> GetCurrentUserAuthorId(CancellationToken ct)
    {
        try
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue) return null;

            var query = new GetAuthorByUserIdQuery(userId.Value);
            var result = await _mediator.Send(query, ct);

            return result.IsSucceeded ? result.Value?.Id : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting author id for user {UserId}", _currentUser.UserId);
            return null;
        }
    }

    private async Task<bool> CanEditBook(Guid bookId, CancellationToken ct)
    {
        try
        {
            // Admins edit everything.
            if (_currentUser.IsInRole("Admin") || _currentUser.IsInRole("SuperAdmin"))
                return true;

            // Author must own the book.
            var authorId = await GetCurrentUserAuthorId(ct);
            if (!authorId.HasValue) return false;

            var bookResult = await _mediator.Send(new GetBookByIdQuery(bookId), ct);
            if (bookResult.IsFailed || bookResult.Value is null) return false;

            return bookResult.Value.AuthorId == authorId.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permissions for book {BookId}", bookId);
            return false;
        }
    }
}

// -------------------------
// Requests (kept here so controller compiles immediately)
// If you prefer: move them to Models/Requests as separate files.
// -------------------------

public sealed class SetBookVisualizationSettingsRequest
{
    public string Mode { get; set; } = "None";
    public bool AllowReaderChoice { get; set; }
    public System.Collections.Generic.List<string>? AllowedModes { get; set; }
    public string? Style { get; set; }
    public string? Provider { get; set; }
    public int MaxImagesPerPage { get; set; } = 1;
    public bool AutoGenerateOnPublish { get; set; }
}

public sealed class SetVisualizationPointRequest
{
    public bool IsVisualizationPoint { get; set; }
    public string? AuthorHint { get; set; }
}

public sealed class SetPageVisualizationRequest
{
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public Guid? VisualizationJobId { get; set; }
}
