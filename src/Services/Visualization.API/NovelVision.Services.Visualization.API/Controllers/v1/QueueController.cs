// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Controllers/v1/QueueController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelVision.Services.Visualization.API.Models.Responses;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.API.Controllers.v1;

/// <summary>
/// Queue status API
/// </summary>
[ApiController]
[Route("api/v1/visualization/queue")]
[Produces("application/json")]
[Authorize]
public sealed class QueueController : ControllerBase
{
    private readonly IJobQueueService _queueService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly ILogger<QueueController> _logger;

    public QueueController(
        IJobQueueService queueService,
        IVisualizationCacheService cacheService,
        ILogger<QueueController> logger)
    {
        _queueService = queueService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get queue status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<QueueStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQueueStatus(CancellationToken ct = default)
    {
        // Try cache first
        var cached = await _cacheService.GetQueueStatusAsync(ct);
        if (cached != null)
        {
            return Ok(ApiResponse<QueueStatusDto>.Ok(cached));
        }

        var queueLength = await _queueService.GetQueueLengthAsync(ct);
        var estimatedWait = await _queueService.EstimateWaitTimeAsync(queueLength, ct);

        var status = new QueueStatusDto
        {
            TotalInQueue = queueLength,
            EstimatedWaitTime = estimatedWait,
            UpdatedAt = DateTime.UtcNow
        };

        // Cache for 30 seconds
        await _cacheService.SetQueueStatusAsync(status, ct);

        return Ok(ApiResponse<QueueStatusDto>.Ok(status));
    }

    /// <summary>
    /// Get position of a specific job in queue
    /// </summary>
    [HttpGet("position/{jobId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<JobQueuePositionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobPosition(Guid jobId, CancellationToken ct = default)
    {
        var visualizationJobId = VisualizationJobId.From(jobId);
        var position = await _queueService.GetQueuePositionAsync(visualizationJobId, ct);

        if (position == 0)
        {
            return Ok(ApiResponse<JobQueuePositionDto>.Ok(new JobQueuePositionDto
            {
                JobId = jobId,
                Position = 0,
                IsProcessing = true,
                Message = "Job is currently being processed or completed"
            }));
        }

        var estimatedWait = await _queueService.EstimateWaitTimeAsync(position, ct);

        var result = new JobQueuePositionDto
        {
            JobId = jobId,
            Position = position,
            EstimatedWaitTime = estimatedWait,
            IsProcessing = false
        };

        return Ok(ApiResponse<JobQueuePositionDto>.Ok(result));
    }
}

public sealed record JobQueuePositionDto
{
    public Guid JobId { get; init; }
    public int Position { get; init; }
    public TimeSpan? EstimatedWaitTime { get; init; }
    public bool IsProcessing { get; init; }
    public string? Message { get; init; }
}