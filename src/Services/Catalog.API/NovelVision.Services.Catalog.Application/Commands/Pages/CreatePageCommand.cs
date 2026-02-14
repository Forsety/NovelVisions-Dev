using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Commands.Pages;

public record CreatePageCommand : IRequest<Result<PageDto>>
{
    public Guid ChapterId { get; init; }
    public string Content { get; init; } = string.Empty;
    public int? PageNumber { get; init; }
    public Dictionary<string, object>? VisualizationSettings { get; init; }
    public bool GenerateVisualization { get; init; } = true;
}

public record UpdatePageCommand : IRequest<Result<PageDto>>
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public Dictionary<string, object>? VisualizationSettings { get; init; }
    public bool RegenerateVisualization { get; init; } = false;
}

public record DeletePageCommand(Guid PageId) : IRequest<Result<bool>>;
