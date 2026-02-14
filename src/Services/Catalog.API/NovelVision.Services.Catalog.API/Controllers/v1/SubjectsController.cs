// src/Services/Catalog.API/NovelVision.Services.Catalog.API/Controllers/v1/SubjectsController.cs
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
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.Queries.Subjects;

namespace NovelVision.Services.Catalog.API.Controllers;

/// <summary>
/// REST API Controller for managing book subjects/categories
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class SubjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubjectsController> _logger;

    public SubjectsController(
        IMediator mediator,
        ILogger<SubjectsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all subjects with optional filtering
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SubjectListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] GetSubjectsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting subjects list with filters: {@Request}", request);

            var query = new GetAllSubjectsQuery
            {
                Type = request.Type,
                OnlyRoot = request.OnlyRoot ?? false,
                IncludeEmpty = request.IncludeEmpty ?? true
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve subjects",
                    Errors = result.Errors.Select(e => e.Message).ToList()
                });
            }

            return Ok(new ApiResponse<List<SubjectListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = $"Retrieved {result.Value.Count} subjects"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subjects");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while retrieving subjects"
            });
        }
    }

    /// <summary>
    /// Get subject hierarchy (tree structure)
    /// </summary>
    [HttpGet("hierarchy")]
    [ProducesResponseType(typeof(ApiResponse<List<SubjectHierarchyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubjectHierarchy(
        [FromQuery] string? type = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting subject hierarchy, type: {Type}", type);

            var query = new GetSubjectHierarchyQuery { Type = type };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve hierarchy"
                });
            }

            return Ok(new ApiResponse<List<SubjectHierarchyDto>>
            {
                Success = true,
                Data = result.Value,
                Message = "Subject hierarchy retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subject hierarchy");
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Get subject by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjectById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting subject by ID: {SubjectId}", id);

            var query = new GetSubjectByIdQuery { Id = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();
                if (error?.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
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
                    Message = error?.Message ?? "Failed to retrieve subject"
                });
            }

            return Ok(new ApiResponse<SubjectDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Subject retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subject {SubjectId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Get subject by slug
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjectBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting subject by slug: {Slug}", slug);

            var query = new GetSubjectBySlugQuery(slug);
            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();
                if (error?.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
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
                    Message = error?.Message ?? "Failed to retrieve subject"
                });
            }

            return Ok(new ApiResponse<SubjectDto>
            {
                Success = true,
                Data = result.Value,
                Message = "Subject retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subject by slug {Slug}", slug);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Get books by subject ID
    /// </summary>
    [HttpGet("{id:guid}/books")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResultDto<BookListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooksBySubject(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool descending = true,
        [FromQuery] bool includeChildSubjects = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting books for subject: {SubjectId}", id);

            var query = new GetBooksBySubjectQuery
            {
                SubjectId = id,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                Descending = descending,
                IncludeChildSubjects = includeChildSubjects
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();
                if (error?.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
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
                    Message = error?.Message ?? "Failed to retrieve books"
                });
            }

            return Ok(new ApiResponse<PaginatedResultDto<BookListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = $"Retrieved {result.Value.Items.Count} books"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books for subject {SubjectId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }

    /// <summary>
    /// Get books by subject slug
    /// </summary>
    [HttpGet("by-slug/{slug}/books")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResultDto<BookListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooksBySubjectSlug(
        string slug,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting books for subject slug: {Slug}", slug);

            var query = new GetBooksBySubjectSlugQuery
            {
                Slug = slug,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result.IsFailed)
            {
                var error = result.Errors.FirstOrDefault();
                if (error?.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
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
                    Message = error?.Message ?? "Failed to retrieve books"
                });
            }

            return Ok(new ApiResponse<PaginatedResultDto<BookListDto>>
            {
                Success = true,
                Data = result.Value,
                Message = $"Retrieved {result.Value.Items.Count} books"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books for subject slug {Slug}", slug);
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
            {
                Success = false,
                Message = "An error occurred"
            });
        }
    }
}