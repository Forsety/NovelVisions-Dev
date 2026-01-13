// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Controllers/v1/ImportController.cs
using System;
using System.Collections.Generic;
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
using NovelVision.Services.Catalog.Application.Commands.Import;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.DTOs.Import;
using NovelVision.Services.Catalog.Application.Interfaces;
using ServiceStack;

namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// REST API Controller for importing books from external sources (Gutenberg, OpenLibrary)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Microsoft.AspNetCore.Mvc.Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Authorize(Policy = "RequireAdminRole")]
public class ImportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGutendexService _gutendexService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(
        IMediator mediator,
        IGutendexService gutendexService,
        ICurrentUserService currentUserService,
        ILogger<ImportController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _gutendexService = gutendexService ?? throw new ArgumentNullException(nameof(gutendexService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Import a single book from Project Gutenberg by ID
    /// </summary>
    /// <param name="gutenbergId">Project Gutenberg book ID</param>
    /// <param name="request">Import options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("gutenberg/{gutenbergId:int}")]
    [ProducesResponseType(typeof(ApiResponse<ImportBookResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ImportBookResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImportGutenbergBook(
        int gutenbergId,
        [FromBody] ImportGutenbergBookRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Importing Gutenberg book {GutenbergId} by user {UserId}",
                gutenbergId, _currentUserService.UserId);

            var command = new ImportGutenbergBookCommand
            {
                GutenbergId = gutenbergId,
                ImportFullText = request?.ImportFullText ?? true,
                WordsPerPage = request?.WordsPerPage ?? 300,
                CreateAuthorIfNotExists = request?.CreateAuthorIfNotExists ?? true,
                CreateSubjectsIfNotExist = request?.CreateSubjectsIfNotExist ?? true,
                SkipIfExists = request?.SkipIfExists ?? true,
                UserId = _currentUserService.UserId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Import failed",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            var statusCode = result.Value.AuthorCreated ? StatusCodes.Status201Created : StatusCodes.Status200OK;

            return StatusCode(statusCode, new ApiResponse<ImportBookResultDto>
            {
                Success = true,
                Data = result.Value,
                Message = $"Book '{result.Value.Title}' imported successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Gutenberg book {GutenbergId}", gutenbergId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while importing the book"
            });
        }
    }

    /// <summary>
    /// Bulk import books from Project Gutenberg
    /// </summary>
    /// <param name="request">Bulk import options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("gutenberg/bulk")]
    [ProducesResponseType(typeof(ApiResponse<BulkImportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkImportGutenberg(
        [FromBody] BulkImportGutenbergRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting bulk import from Gutenberg by user {UserId}. Max books: {MaxBooks}",
                _currentUserService.UserId, request.MaxBooks);

            var command = new BulkImportGutenbergCommand
            {
                GutenbergIds = request.GutenbergIds ?? new List<int>(),
                SearchCriteria = request.SearchCriteria is not null ? new GutenbergSearchCriteriaDto
                {
                    Search = request.SearchCriteria.Search,
                    Languages = request.SearchCriteria.Languages,
                    Topic = request.SearchCriteria.Topic,
                    AuthorYearStart = request.SearchCriteria.AuthorYearStart,
                    AuthorYearEnd = request.SearchCriteria.AuthorYearEnd,
                    Copyright = request.SearchCriteria.Copyright
                } : null,
                MaxBooks = request.MaxBooks,
                ImportFullText = request.ImportFullText,
                WordsPerPage = request.WordsPerPage,
                SkipExisting = request.SkipExisting,
                ContinueOnError = request.ContinueOnError,
                DelayBetweenRequests = request.DelayBetweenRequests,
                UserId = _currentUserService.UserId
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Bulk import failed",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<BulkImportResultDto>
            {
                Success = true,
                Data = result.Value,
                Message = $"Bulk import completed: {result.Value.SuccessCount} succeeded, {result.Value.FailedCount} failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk import from Gutenberg");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred during bulk import"
            });
        }
    }

    /// <summary>
    /// Search books in Project Gutenberg (without importing)
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("gutenberg/search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<GutenbergSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchGutenberg(
        [FromQuery] SearchGutenbergRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching Gutenberg: {Query}", request.Search);

            var criteria = new GutenbergSearchCriteriaDto
            {
                Search = request.Search,
                Languages = request.Languages?.Split(',').ToList(),
                Topic = request.Topic,
                AuthorYearStart = request.AuthorYearStart,
                AuthorYearEnd = request.AuthorYearEnd,
                Copyright = request.Copyright,
                Page = request.Page,
                Sort = request.Sort
            };

            var result = await _gutendexService.SearchBooksAsync(criteria, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Search failed"
                });
            }

            return Ok(new ApiResponse<GutenbergSearchResultDto>
            {
                Success = true,
                Data = result.Value,
                Message = $"Found {result.Value.Count} books"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Gutenberg");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while searching Gutenberg"
            });
        }
    }

    /// <summary>
    /// Get popular books from Project Gutenberg
    /// </summary>
    [HttpGet("gutenberg/popular")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<GutenbergSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularGutenbergBooks(
        [FromQuery] int page = 1,
        [FromQuery] string? language = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _gutendexService.GetPopularBooksAsync(page, language, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to get popular books"
                });
            }

            return Ok(new ApiResponse<GutenbergSearchResultDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Popular books retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular Gutenberg books");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Get a single book preview from Gutenberg (without importing)
    /// </summary>
    [HttpGet("gutenberg/{gutenbergId:int}/preview")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<GutenbergBookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PreviewGutenbergBook(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _gutendexService.GetBookAsync(gutenbergId, cancellationToken);

            if (result.IsFailed)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Book not found"
                });
            }

            return Ok(new ApiResponse<GutenbergBookDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Book preview retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Gutenberg book preview {Id}", gutenbergId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Check Gutendex service availability
    /// </summary>
    [HttpGet("gutenberg/health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckGutendexHealth(CancellationToken cancellationToken = default)
    {
        var isAvailable = await _gutendexService.IsAvailableAsync(cancellationToken);

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Data = isAvailable,
            Message = isAvailable ? "Gutendex service is available" : "Gutendex service is unavailable"
        });
    }
}