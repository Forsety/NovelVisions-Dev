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
using NovelVision.Services.Catalog.Application.Commands.Authors;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.Queries.Authors;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// REST API Controller for managing authors
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AuthorsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthorsController> _logger;

    public AuthorsController(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<AuthorsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a paginated list of authors
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResultDto<AuthorListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAuthors(
        [FromQuery] GetAuthorsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting authors list with filters: {@Request}", request);

            var query = new GetAuthorsQuery
            {
                Verified = request.Verified,
                SearchTerm = request.SearchTerm,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 20
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve authors",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<PaginatedResultDto<AuthorListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = "Authors retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving authors");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving authors"
            });
        }
    }

    /// <summary>
    /// Get detailed information about a specific author
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AuthorDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuthorById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting author details for ID: {AuthorId}", id);

            var query = new GetAuthorByIdQuery(id);
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
                    Message = error?.Message ?? "Failed to retrieve author",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<AuthorDetailDto>
            {
                Success = true,
                Data = (AuthorDetailDto)result.Value,
                Message = "Author retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving author with ID: {AuthorId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the author"
            });
        }
    }

    /// <summary>
    /// Register a new author
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAuthor(
    [FromBody] CreateAuthorRequest request,
    CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating new author: {DisplayName} for user: {UserId}",
                request.DisplayName, _currentUserService.UserId);

            // Получаем Guid? из текущего пользователя
            var userId = _currentUserService.UserId; // Guid?

            if (!userId.HasValue)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "User ID is missing"
                });
            }

            var command = new CreateAuthorCommand
            {
                UserId = userId.Value,
                DisplayName = request.DisplayName,
                Email = request.Email,
                Biography = request.Biography,
                SocialLinks = request.SocialLinks ?? new Dictionary<string, string>()
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();

                if (error != null && (error.Message.Contains("already exists") || error.Message.Contains("conflict")))
                {
                    return Conflict(new ApiResponse
                    {
                        Success = false,
                        Message = error.Message
                    });
                }

                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = error?.Message ?? "Failed to create author",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return CreatedAtAction(
                nameof(GetAuthorById),
                new { id = result.Value.Id, version = "1.0" },
                new ApiResponse<AuthorDto>
                {
                    Success = true,
                    Data = result.Value,
                    Message = "Author created successfully"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating author");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while creating the author"
            });
        }
    }


    /// <summary>
    /// Update author profile
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAuthor(
        Guid id,
        [FromBody] UpdateAuthorRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating author: {AuthorId} by user: {UserId}",
                id, _currentUserService.UserId);

            // Check if user can edit this author profile
            if (!await CanEditAuthor(id, cancellationToken))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ApiResponse
                {
                    Success = false,
                    Message = "You don't have permission to edit this author profile"
                });
            }

            var command = new UpdateAuthorCommand
            {
                Id = id,
                DisplayName = request.DisplayName,
                Biography = request.Biography,
                SocialLinks = request.SocialLinks ?? new Dictionary<string, string>()
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
                    Message = error?.Message ?? "Failed to update author",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<AuthorDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Author updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating author with ID: {AuthorId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while updating the author"
            });
        }
    }

    /// <summary>
    /// Verify an author (Admin only)
    /// </summary>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyAuthor(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verifying author: {AuthorId} by admin: {UserId}",
                id, _currentUserService.UserId);

            var command = new VerifyAuthorCommand(id);
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
                    Message = error?.Message ?? "Failed to verify author",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Author verified successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying author with ID: {AuthorId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while verifying the author"
            });
        }
    }

    #region Private Helper Methods

    private async Task<bool> CanEditAuthor(Guid authorId, CancellationToken cancellationToken)
    {
        try
        {
            if (_currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("SuperAdmin"))
                return true;

            var query = new GetAuthorByIdQuery(authorId);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed || result.Value == null)
                return false;

            // Было Guid.TryParse(string) — заменяем на работу с Guid?
            var userId = _currentUserService.UserId; // Guid?
            if (!userId.HasValue)
                return false;

            return result.Value.Id == userId.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permissions for author: {AuthorId}", authorId);
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
