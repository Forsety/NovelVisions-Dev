using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Services;

public interface IVisualizationService
{
    Task<Result<VisualizationPlanDto>> CreateVisualizationPlanAsync(
        Guid bookId,
        CancellationToken cancellationToken = default);
    
    Task<Result<List<string>>> GeneratePromptsForChapterAsync(
        Guid bookId,
        Guid chapterId,
        CancellationToken cancellationToken = default);
}

public record VisualizationPlanDto
{
    public Guid BookId { get; init; }
    public string Mode { get; init; } = string.Empty;
    public int TotalImages { get; init; }
    public List<VisualizationTargetDto> Targets { get; init; } = new();
}

public record VisualizationTargetDto
{
    public string Id { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
}
