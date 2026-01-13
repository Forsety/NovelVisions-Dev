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
using NovelVision.Services.Catalog.Application.Commands.Books;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.Queries.Books;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.Queries.Subjects;
namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// REST API Controller for managing books in the catalog
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<BooksController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a paginated list of books with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResultDto<BookListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBooks(
        [FromQuery] GetBooksRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting books list with filters: {@Request}", request);

            var query = new SearchBooksQuery
            {
                SearchTerm = request.SearchTerm,
                Status = request.Status,
                Genre = request.Genre,
                Language = request.Language,
                MinPages = request.MinPages,
                MaxPages = request.MaxPages,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 20,
                AuthorId = request.AuthorId
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve books",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<PaginatedResultDto<BookListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = "Books retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving books"
            });
        }
    }

    /// <summary>
    /// Get detailed information about a specific book
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting book details for ID: {BookId}", id);

            var query = new GetBookByIdQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();

                // Check if it's a "not found" error by examining the error message or code
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
                    Message = error?.Message ?? "Failed to retrieve book",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<BookDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Book retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving book with ID: {BookId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the book"
            });
        }
    }

    /// <summary>
    /// Create a new book
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAuthorRole")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBook(
        [FromBody] CreateBookRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new book: {Title} by user: {UserId}",
                request.Title, _currentUserService.UserId);

            // Get the author ID for the current user
            var authorId = await GetCurrentUserAuthorId(cancellationToken);
            if (!authorId.HasValue)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "User is not registered as an author"
                });
            }

            var command = new CreateBookCommand
            {
                Title = request.Title,
                Subtitle = request.Subtitle,
                Description = request.Description,
                LanguageCode = request.LanguageCode ?? "en",
                AuthorId = authorId.Value,
                ISBN = request.ISBN,
                Publisher = request.Publisher,
                PublicationDate = request.PublicationDate,
                Edition = request.Edition,
                Genres = request.Genres ?? new List<string>(),
                Tags = request.Tags ?? new List<string>()
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to create book",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return CreatedAtAction(
                nameof(GetBookById),
                new { id = result.Value.Id, version = "1.0" },
                new ApiResponse<BookDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "Book created successfully"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while creating the book"
            });
        }
    }

    /// <summary>
    /// Update book information
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAuthorRole")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating book: {BookId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Check ownership or admin role
            var canEdit = await CanEditBook(id, cancellationToken);
            if (!canEdit)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to edit this book"
                });
            }

            var command = new UpdateBookCommand
            {
                Id = id,
                Title = request.Title,
                Subtitle = request.Subtitle,
                Description = request.Description,
                Publisher = request.Publisher,
                PublicationDate = request.PublicationDate,
                Edition = request.Edition,
                Genres = request.Genres ?? new List<string>(),
                Tags = request.Tags ?? new List<string>()
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
                    Message = error?.Message ?? "Failed to update book",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<BookDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Book updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book with ID: {BookId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while updating the book"
            });
        }
    }

    /// <summary>
    /// Delete a book (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAuthorRole")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteBook(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting book: {BookId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Check ownership or admin role
            var canDelete = await CanEditBook(id, cancellationToken);
            if (!canDelete)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to delete this book"
                });
            }

            var command = new Application.Queries.Books.DeleteBookCommand(id);
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
                    Message = error?.Message ?? "Failed to delete book",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Book deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book with ID: {BookId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while deleting the book"
            });
        }
    }

    /// <summary>
    /// Publish a book
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "RequireAuthorRole")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishBook(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Publishing book: {BookId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Check ownership
            var canPublish = await CanEditBook(id, cancellationToken);
            if (!canPublish)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to publish this book"
                });
            }

            var command = new PublishBookCommand(id);
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

                // Validation errors (not enough chapters/pages)
                if (error != null && IsValidationError(error))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Book does not meet publication requirements",
                        Errors = result.Errors.Select(e => e.Message).ToList()
                    });
                }

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Failed to publish book",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Book published successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing book with ID: {BookId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while publishing the book"
            });
        }
    }

    
    [HttpGet("{id:guid}/chapters")]
    [ProducesResponseType(typeof(ApiResponse<List<ChapterListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookChapters(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting chapters for book: {BookId}", id);

            var query = new GetBookChaptersQuery(id);
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
                    Message = error?.Message ?? "Failed to retrieve chapters",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<List<ChapterListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = "Chapters retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chapters for book ID: {BookId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving chapters"
            });
        }
    }
    [HttpGet("search/advanced")]
    [ProducesResponseType(typeof(ApiResponse<Application.DTOs.SearchResultDto<BookListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchBooksAdvanced(
        [FromQuery] SearchBooksAdvancedRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Advanced search with filters: {@Request}", request);

            var query = new SearchBooksAdvancedQuery
            {
                SearchTerm = request.SearchTerm,
                SearchInTitle = request.SearchInTitle,
                SearchInDescription = request.SearchInDescription,
                SearchInAuthor = request.SearchInAuthor,
                SubjectIds = request.SubjectIds,
                Genres = request.Genres,
                Languages = request.Languages,
                CopyrightStatus = request.CopyrightStatus,
                Source = request.Source,
                IsFreeToUse = request.IsFreeToUse,
                IsImported = request.IsImported,
                MinPageCount = request.MinPageCount,
                MaxPageCount = request.MaxPageCount,
                MinDownloadCount = request.MinDownloadCount,
                PublishedAfter = request.PublishedAfter,
                PublishedBefore = request.PublishedBefore,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortBy = request.SortBy,
                Descending = request.Descending
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Search failed",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<Application.Queries.Books.SearchResultDto<BookListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = $"Found {result.Value.TotalCount} books"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced search");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred during search"
            });
        }
    }

    /// <summary>
    /// Get popular books
    /// </summary>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(ApiResponse<List<BookListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularBooks(
        [FromQuery] int count = 20,
        [FromQuery] string? language = null,
        [FromQuery] bool onlyFreeToUse = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting {Count} popular books", count);

            var query = new GetPopularBooksQuery
            {
                Count = count,
                Language = language,
                OnlyFreeToUse = onlyFreeToUse
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to get popular books"
                });
            }

            return Ok(new ApiResponse<List<BookListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = $"Retrieved {result.Value.Count} popular books"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular books");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Get recently added books
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(ApiResponse<List<BookListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentBooks(
        [FromQuery] int count = 20,
        [FromQuery] string? language = null,
        [FromQuery] bool onlyImported = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting {Count} recent books", count);

            var query = new GetRecentBooksQuery
            {
                Count = count,
                Language = language,
                OnlyImported = onlyImported
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to get recent books"
                });
            }

            return Ok(new ApiResponse<List<BookListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = $"Retrieved {result.Value.Count} recent books"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent books");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Get books by source (Gutenberg, OpenLibrary, etc.)
    /// </summary>
    [HttpGet("by-source/{source}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResultDto<BookListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBooksBySource(
        string source,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting books by source: {Source}", source);

            var query = new GetBooksBySourceQuery
            {
                Source = source,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to get books"
                });
            }

            return Ok(new ApiResponse<PaginatedResultDto<BookListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = $"Retrieved {result.Value.Items.Count} books from {source}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting books by source {Source}", source);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }
    #region Private Helper Methods

    /// <summary>
    /// Get the author ID for the current user
    /// </summary>
    private async Task<Guid?> GetCurrentUserAuthorId(CancellationToken cancellationToken)
    {
        try
        {
            Guid? userId = _currentUserService.UserId; // Guid?

            if (!userId.HasValue)
                return null;

            var query = new GetAuthorByUserIdQuery(userId.Value);
            var result = await _mediator.Send(query, cancellationToken);

            return result.IsSucceeded ? result.Value?.Id : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting author ID for user: {UserId}", _currentUserService.UserId);
            return null;
        }
    }




    /// <summary>
    /// Check if current user can edit a book
    /// </summary>
    private async Task<bool> CanEditBook(Guid bookId, CancellationToken cancellationToken)
    {
        try
        {
            // Admins can edit any book
            if (_currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("SuperAdmin"))
                return true;

            // Get the book to check ownership
            var query = new GetBookByIdQuery(bookId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed || result.Value == null)
                return false;

            // Check if current user is the author
            var currentAuthorId = await GetCurrentUserAuthorId(cancellationToken);
            return currentAuthorId.HasValue && result.Value.AuthorId == currentAuthorId.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permissions for book: {BookId}", bookId);
            return false;
        }
    }

    /// <summary>
    /// Check if error is a "not found" error
    /// </summary>
    private bool IsNotFoundError(Error error)
    {
        // Check by error message pattern or custom property if available
        return error.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
               error.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if error is a validation error
    /// </summary>
    private bool IsValidationError(Error error)
    {
        // Check by error message pattern
        return error.Message.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
               error.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
               error.Message.Contains("required", StringComparison.OrdinalIgnoreCase) ||
               error.Message.Contains("must", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}