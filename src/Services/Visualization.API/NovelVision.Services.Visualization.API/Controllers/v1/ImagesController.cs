// src/Services/Visualization.API/NovelVision.Services.Visualization.API/Controllers/v1/ImagesController.cs

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovelVision.Services.Visualization.API.Models.Responses;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Queries.GetBookImages;
using NovelVision.Services.Visualization.Application.Queries.GetPageImages;

namespace NovelVision.Services.Visualization.API.Controllers.v1;

/// <summary>
/// Images API
/// </summary>
[ApiController]
[Route("api/v1/visualization/images")]
[Produces("application/json")]
[Authorize]
public sealed class ImagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IMediator mediator,
        ILogger<ImagesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get images for a book
    /// </summary>
    [HttpGet("book/{bookId:guid}")]
    [ProducesResponseType(typeof(PaginatedResponse<GeneratedImageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookImages(
        Guid bookId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting images for BookId: {BookId}", bookId);

        var query = new GetBookImagesQuery
        {
            BookId = bookId,
            Skip = (page - 1) * pageSize,
            Take = pageSize
        };

        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        var response = new PaginatedResponse<GeneratedImageDto>
        {
            Success = true,
            Data = result.Value.Images,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.Value.TotalCount
        };

        return Ok(response);
    }

    /// <summary>
    /// Get images for a page
    /// </summary>
    [HttpGet("page/{pageId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GeneratedImageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPageImages(Guid pageId, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting images for PageId: {PageId}", pageId);

        var query = new GetPageImagesQuery { PageId = pageId };
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponse.Fail(result.Error.Message));
        }

        return Ok(ApiResponse<IReadOnlyList<GeneratedImageDto>>.Ok(result.Value));
    }

    /// <summary>
    /// Get selected image for a page
    /// </summary>
    [HttpGet("page/{pageId:guid}/selected")]
    [ProducesResponseType(typeof(ApiResponse<GeneratedImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSelectedImage(Guid pageId, CancellationToken ct = default)
    {
        var query = new GetSelectedImageQuery { PageId = pageId };
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            return NotFound(ApiResponse.Fail(result.Error.Message));
        }

        return Ok(ApiResponse<GeneratedImageDto>.Ok(result.Value));
    }
}