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
using NovelVision.Services.Catalog.Application.Commands.Pages;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.Queries.Chapters;
using NovelVision.Services.Catalog.Application.Queries.Pages;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.Commands.Books;
using NovelVision.Services.Catalog.Application.Queries.Books;

namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// REST API Controller for managing chapter pages
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Produces("application/json")]
public class PagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PagesController> _logger;

    public PagesController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<PagesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Add a new page to a chapter
    /// </summary>
    [HttpPost("chapters/{chapterId:guid}/pages")]
    [Authorize(Policy = "RequireAuthorRole")]
    [ProducesResponseType(typeof(ApiResponse<PageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreatePage(
        Guid chapterId,
        [FromBody] CreatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new page for chapter: {ChapterId} by user: {UserId}",
                chapterId, _currentUserService.UserId);

            // Check if user can edit this chapter
            if (!await CanEditChapter(chapterId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to add pages to this chapter"
                });
            }

            var command = new CreatePageCommand
            {
                ChapterId = chapterId,
                Content = request.Content,
                PageNumber = request.PageNumber, // If null, will be auto-calculated
                VisualizationSettings = request.VisualizationSettings,
                GenerateVisualization = request.GenerateVisualization ?? true
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
                    Message = error?.Message ?? "Failed to create page",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return CreatedAtAction(
                nameof(GetPageById),
                new { id = result.Value.Id, version = "1.0" },
                new ApiResponse<PageDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "Page created successfully"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating page for chapter: {ChapterId}", chapterId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while creating the page"
            });
        }
    }

    /// <summary>
    /// Get page by ID
    /// </summary>
    [HttpGet("pages/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPageById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting page details for ID: {PageId}", id);

            var query = new GetPageByIdQuery(id);
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
                    Message = error?.Message ?? "Failed to retrieve page",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            // Check if the book is published or user has access
            if (!await CanViewPage(result.Value, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to view this page"
                });
            }

            return Ok(new ApiResponse<PageDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Page retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving page with ID: {PageId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the page"
            });
        }
    }

    /// <summary>
    /// Update a page including content and visualization
    /// </summary>
    [HttpPut("pages/{id:guid}")]
    [Authorize(Policy = "RequireAuthorRole")]
    [ProducesResponseType(typeof(ApiResponse<PageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePage(
        Guid id,
        [FromBody] UpdatePageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating page: {PageId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Get page to check chapter ownership
            var pageQuery = new GetPageByIdQuery(id);
            var pageResult = await _mediator.Send(pageQuery, cancellationToken);

            if (pageResult.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Page not found"
                });
            }

            // Check if user can edit the chapter this page belongs to
            if (!await CanEditChapter(pageResult.Value.ChapterId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to edit this page"
                });
            }

            var command = new Application.Commands.Pages.UpdatePageCommand
            {
                Id = id,
                Content = request.Content,
                VisualizationSettings = request.VisualizationSettings,
                RegenerateVisualization = request.RegenerateVisualization ?? false
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to update page",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<PageDto>
            {
                Success = true,
                Data = result.Value,
                Message = request.RegenerateVisualization == true
                    ? "Page updated and visualization regenerated successfully"
                    : "Page updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page with ID: {PageId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while updating the page"
            });
        }
    }

    /// <summary>
    /// Get visualization data for a page
    /// </summary>
    [HttpGet("pages/{id:guid}/visualization")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPageVisualization(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting visualization for page: {PageId}", id);

            // First get the page to check permissions
            var pageQuery = new GetPageByIdQuery(id);
            var pageResult = await _mediator.Send(pageQuery, cancellationToken);

            if (pageResult.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Page not found"
                });
            }

            // Check if user can view this page
            if (!await CanViewPage(pageResult.Value, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to view this visualization"
                });
            }

            var query = new GetPageVisualizationQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();

                if (error != null && error.Message.Contains("not enabled"))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Visualization is not enabled for this book"
                    });
                }

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Failed to retrieve visualization",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<VisualizationDataDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Visualization data retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving visualization for page: {PageId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving visualization data"
            });
        }
    }

    /// <summary>
    /// Delete a page from a chapter
    /// </summary>
    [HttpDelete("pages/{id:guid}")]
    [Authorize(Policy = "RequireAuthorRole")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting page: {PageId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Get page to check chapter ownership
            var pageQuery = new GetPageByIdQuery(id);
            var pageResult = await _mediator.Send(pageQuery, cancellationToken);

            if (pageResult.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Page not found"
                });
            }

            // Check if user can edit the chapter this page belongs to
            if (!await CanEditChapter(pageResult.Value.ChapterId, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to delete this page"
                });
            }

            var command = new DeletePageCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete page",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Page deleted successfully. Page numbers have been recalculated."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting page with ID: {PageId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting the page"
            });
        }
    }

    #region Private Helper Methods

    private async Task<bool> CanEditChapter(Guid chapterId, CancellationToken cancellationToken)
    {
        try
        {
            // Get chapter details
            var chapterQuery = new GetChapterByIdQuery(chapterId);
            var chapterResult = await _mediator.Send(chapterQuery, cancellationToken);

            if (chapterResult.IsFailed)
                return false;

            // Check if user can edit the book
            return await CanEditBook(chapterResult.Value.BookId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permissions for chapter: {ChapterId}", chapterId);
            return false;
        }
    }

    private async Task<bool> CanEditBook(Guid bookId, CancellationToken cancellationToken)
    {
        try
        {
            // Admins can edit any book
            if (_currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("SuperAdmin"))
                return true;

            // Get the book to check ownership
            var result = await _mediator.Send(new GetBookByIdQuery(bookId), cancellationToken);
            if (result.IsFailed || result.Value == null)
                return false;

            // Get current user's author ID
            var userGuid = _currentUserService.UserId; // Guid?
            if (!userGuid.HasValue)
                return false;

            var authorResult = await _mediator.Send(new GetAuthorByUserIdQuery(userGuid.Value), cancellationToken);
            if (authorResult.IsFailed || authorResult.Value == null)
                return false;

            // Check if current user is the author of the book
            return result.Value.AuthorId == authorResult.Value.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permissions for book: {BookId}", bookId);
            return false;
        }
    }


    private async Task<bool> CanViewPage(PageDto page, CancellationToken cancellationToken)
    {
        try
        {
            // If the book is published, everyone can view
            // Otherwise, only author and admins can view
            // This would require checking the book status through the chapter

            // For now, allow viewing if user is authenticated
            // You can implement more complex logic here
            return true;
        }
        catch
        {
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
