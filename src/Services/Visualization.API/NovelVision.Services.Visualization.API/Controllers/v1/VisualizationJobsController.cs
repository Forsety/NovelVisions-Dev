// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Controllers/v1/VisualizationJobsController.cs

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelVision.Services.Visualization.API.Models.Responses;
using NovelVision.Services.Visualization.Application.Commands.CancelJob;
using NovelVision.Services.Visualization.Application.Commands.CreateVisualizationJob;
using NovelVision.Services.Visualization.Application.Commands.DeleteImage;
using NovelVision.Services.Visualization.Application.Commands.RetryJob;
using NovelVision.Services.Visualization.Application.Commands.SelectImage;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Application.Queries.GetJob;
using NovelVision.Services.Visualization.Application.Queries.GetUserJobs;

namespace NovelVision.Services.Visualization.API.Controllers.v1;

/// <summary>
/// Visualization Jobs API
/// </summary>
[ApiController]
[Route("api/v1/visualization/jobs")]
[Produces("application/json")]
[Authorize]
public sealed class VisualizationJobsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<VisualizationJobsController> _logger;

    public VisualizationJobsController(
        IMediator mediator,
        ICurrentUserService currentUser,
        ILogger<VisualizationJobsController> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Create page visualization job
    /// </summary>
    [HttpPost("page")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationJobDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePageVisualization(
        [FromBody] CreatePageVisualizationRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating page visualization for BookId: {BookId}, PageId: {PageId}",
            request.BookId, request.PageId);

        var command = new CreatePageVisualizationCommand
        {
            BookId = request.BookId,
            PageId = request.PageId,
            UserId = _currentUser.UserId,
            PreferredProvider = request.PreferredProvider,
            Parameters = request.Parameters != null ? new GenerationParametersDto
            {
                Size = request.Parameters.Size,
                Quality = request.Parameters.Quality,
                AspectRatio = request.Parameters.AspectRatio
            } : null
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        return CreatedAtAction(
            nameof(GetJob),
            new { jobId = result.Value.Id },
            ApiResponse<VisualizationJobDto>.Ok(result.Value, "Visualization job created"));
    }

    /// <summary>
    /// Create text selection visualization job
    /// </summary>
    [HttpPost("text-selection")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationJobDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTextSelectionVisualization(
        [FromBody] CreateTextSelectionVisualizationRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating text selection visualization for BookId: {BookId}",
            request.BookId);

        var command = new CreateTextSelectionVisualizationCommand
        {
            BookId = request.BookId,
            PageId = request.PageId,
            UserId = _currentUser.UserId,
            SelectedText = request.SelectedText,
            StartPosition = request.StartPosition,
            EndPosition = request.EndPosition,
            ContextBefore = request.ContextBefore,
            ContextAfter = request.ContextAfter,
            PreferredProvider = request.PreferredProvider
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        return CreatedAtAction(
            nameof(GetJob),
            new { jobId = result.Value.Id },
            ApiResponse<VisualizationJobDto>.Ok(result.Value, "Visualization job created"));
    }

    /// <summary>
    /// Get visualization job by ID
    /// </summary>
    [HttpGet("{jobId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid jobId, CancellationToken ct = default)
    {
        var query = new GetJobQuery { JobId = jobId };
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            return NotFound(ApiResponse.Fail(result.Error.Message));
        }

        return Ok(ApiResponse<VisualizationJobDto>.Ok(result.Value));
    }

    /// <summary>
    /// Get current user's jobs
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PaginatedResponse<VisualizationJobSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetUserJobsQuery
        {
            UserId = _currentUser.UserId,
            Skip = (page - 1) * pageSize,
            Take = pageSize
        };

        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        var response = new PaginatedResponse<VisualizationJobSummaryDto>
        {
            Success = true,
            Data = result.Value.Jobs,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.Value.TotalCount
        };

        return Ok(response);
    }

    /// <summary>
    /// Cancel a visualization job
    /// </summary>
    [HttpPost("{jobId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelJob(Guid jobId, CancellationToken ct = default)
    {
        var command = new CancelJobCommand
        {
            JobId = jobId,
            UserId = _currentUser.UserId
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        return Ok(ApiResponse.Ok("Job cancelled successfully"));
    }

    /// <summary>
    /// Retry a failed job
    /// </summary>
    [HttpPost("{jobId:guid}/retry")]
    [ProducesResponseType(typeof(ApiResponse<VisualizationJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RetryJob(Guid jobId, CancellationToken ct = default)
    {
        var command = new RetryJobCommand
        {
            JobId = jobId,
            UserId = _currentUser.UserId
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        return Ok(ApiResponse<VisualizationJobDto>.Ok(result.Value, "Job retry initiated"));
    }

    /// <summary>
    /// Select an image as the main visualization
    /// </summary>
    [HttpPost("{jobId:guid}/images/{imageId:guid}/select")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SelectImage(
        Guid jobId,
        Guid imageId,
        CancellationToken ct = default)
    {
        var command = new SelectImageCommand
        {
            JobId = jobId,
            ImageId = imageId,
            UserId = _currentUser.UserId
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        return Ok(ApiResponse.Ok("Image selected successfully"));
    }

    /// <summary>
    /// Delete a generated image
    /// </summary>
    [HttpDelete("{jobId:guid}/images/{imageId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteImage(
        Guid jobId,
        Guid imageId,
        CancellationToken ct = default)
    {
        var command = new DeleteImageCommand
        {
            JobId = jobId,
            ImageId = imageId,
            UserId = _currentUser.UserId
        };

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        return Ok(ApiResponse.Ok("Image deleted successfully"));
    }
}

#region Request DTOs

public sealed record CreatePageVisualizationRequest
{
    public Guid BookId { get; init; }
    public Guid PageId { get; init; }
    public string? PreferredProvider { get; init; }
    public GenerationParametersRequest? Parameters { get; init; }
}

public sealed record CreateTextSelectionVisualizationRequest
{
    public Guid BookId { get; init; }
    public Guid PageId { get; init; }
    public string SelectedText { get; init; } = string.Empty;
    public int StartPosition { get; init; }
    public int EndPosition { get; init; }
    public string? ContextBefore { get; init; }
    public string? ContextAfter { get; init; }
    public string? PreferredProvider { get; init; }
}

public sealed record GenerationParametersRequest
{
    public string? Size { get; init; }
    public string? Quality { get; init; }
    public string? AspectRatio { get; init; }
}

#endregion