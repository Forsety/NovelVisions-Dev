using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.API.Models.Requests;
using NovelVision.Services.Catalog.API.Models.Responses;
using NovelVision.Services.Catalog.Application.Commands.Chapters;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.Queries.Books;
using NovelVision.Services.Catalog.Application.Queries.Chapters;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.Commands.Books;

namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// REST API Controller for managing book chapters
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Produces("application/json")]
[Authorize(Policy = "RequireAuthorRole")]
public class ChaptersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ChaptersController> _logger;

    public ChaptersController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<ChaptersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Add a new chapter to a book
    /// </summary>
    [HttpPost("books/{bookId:guid}/chapters")]
    [ProducesResponseType(typeof(ApiResponse<ChapterDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateChapter(
        Guid bookId,
        [FromBody] CreateChapterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new chapter for book: {BookId} by user: {UserId}",
                bookId, _currentUserService.UserId);

            // Check if user can edit this book
            if (!await CanEditBook(bookId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to add chapters to this book"
                });
            }

            var command = new CreateChapterCommand
            {
                BookId = bookId,
                Title = request.Title,
                Summary = request.Summary,
                OrderIndex = request.OrderIndex // If null, will be auto-calculated
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();

                if (error != null && IsNotFoundError(error))
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
                    Message = error?.Message ?? "Failed to create chapter",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return CreatedAtAction(
                nameof(GetChapterById),
                new { id = result.Value.Id, version = "1.0" },
                new ApiResponse<ChapterDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "Chapter created successfully"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chapter for book: {BookId}", bookId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while creating the chapter"
            });
        }
    }

    /// <summary>
    /// Get chapter by ID
    /// </summary>
    [HttpGet("chapters/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChapterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChapterById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting chapter details for ID: {ChapterId}", id);

            var query = new GetChapterByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();

                if (error != null && IsNotFoundError(error))
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
                    Message = error?.Message ?? "Failed to retrieve chapter",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<ChapterDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Chapter retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chapter with ID: {ChapterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the chapter"
            });
        }
    }

    /// <summary>
    /// Update a chapter
    /// </summary>
    [HttpPut("chapters/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ChapterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChapter(
        Guid id,
        [FromBody] UpdateChapterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating chapter: {ChapterId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Get chapter to check book ownership
            var chapterQuery = new GetChapterByIdQuery(id);
            var chapterResult = await _mediator.Send(chapterQuery, cancellationToken);

            if (chapterResult.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Chapter not found"
                });
            }

            // Check if user can edit the book this chapter belongs to
            if (!await CanEditBook(chapterResult.Value.BookId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to edit this chapter"
                });
            }

            var command = new Application.Commands.Chapters.UpdateChapterCommand
            {
                Id = id,
                Title = request.Title,
                Summary = request.Summary
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to update chapter",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<ChapterDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Chapter updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chapter with ID: {ChapterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while updating the chapter"
            });
        }
    }

    /// <summary>
    /// Delete a chapter
    /// </summary>
    [HttpDelete("chapters/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteChapter(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting chapter: {ChapterId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Get chapter to check book ownership
            var chapterQuery = new GetChapterByIdQuery(id);
            var chapterResult = await _mediator.Send(chapterQuery, cancellationToken);

            if (chapterResult.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Chapter not found"
                });
            }

            // Check if user can edit the book this chapter belongs to
            if (!await CanEditBook(chapterResult.Value.BookId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to delete this chapter"
                });
            }

            var command = new DeleteChapterCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete chapter",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Chapter deleted successfully. Order indices have been recalculated."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chapter with ID: {ChapterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting the chapter"
            });
        }
    }

    /// <summary>
    /// Reorder a chapter within the book
    /// </summary>
    [HttpPost("chapters/{id:guid}/reorder")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderChapter(
        Guid id,
        [FromBody] ReorderChapterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reordering chapter: {ChapterId} to position: {NewIndex}",
                id, request.NewOrderIndex);

            // Get chapter to check book ownership
            var chapterQuery = new GetChapterByIdQuery(id);
            var chapterResult = await _mediator.Send(chapterQuery, cancellationToken);

            if (chapterResult.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Chapter not found"
                });
            }

            // Check if user can edit the book this chapter belongs to
            if (!await CanEditBook(chapterResult.Value.BookId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to reorder this chapter"
                });
            }

            var command = new ReorderChapterCommand
            {
                ChapterId = id,
                NewOrderIndex = request.NewOrderIndex
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to reorder chapter",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"Chapter successfully moved to position {request.NewOrderIndex}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering chapter with ID: {ChapterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while reordering the chapter"
            });
        }
    }

    #region Private Helper Methods

    private async Task<bool> CanEditBook(Guid bookId, CancellationToken cancellationToken)
    {
        try
        {
            // Admins can edit any book
            if (_currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("SuperAdmin"))
                return true;

            // Book must exist
            var bookResult = await _mediator.Send(new GetBookByIdQuery(bookId), cancellationToken);
            if (bookResult.IsFailed || bookResult.Value == null)
                return false;

            // Current user must have Guid
            var userGuid = _currentUserService.UserId; // Guid?
            if (!userGuid.HasValue)
                return false;

            // Get author by user id
            var authorResult = await _mediator.Send(new GetAuthorByUserIdQuery(userGuid.Value), cancellationToken);
            if (authorResult.IsFailed || authorResult.Value == null)
                return false;

            // Owns the book?
            return bookResult.Value.AuthorId == authorResult.Value.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permissions for book: {BookId}", bookId);
            return false;
        }
    }


    private bool IsNotFoundError(Error error)
    {
        return error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
               error.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
