using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;

namespace NovelVision.Services.Visualization.Application.Commands.CreateVisualizationJob;

/// <summary>
/// Команда создания задания визуализации для страницы (кнопка)
/// </summary>
public sealed record CreatePageVisualizationCommand : IRequest<Result<VisualizationJobDto>>
{
    public Guid BookId { get; init; }
    public Guid PageId { get; init; }
    public Guid UserId { get; init; }
    public string? PreferredProvider { get; init; }
    public GenerationParametersDto? Parameters { get; init; }
}

/// <summary>
/// Команда создания задания визуализации для выделенного текста
/// </summary>
public sealed record CreateTextSelectionVisualizationCommand : IRequest<Result<VisualizationJobDto>>
{
    public Guid BookId { get; init; }
    public Guid UserId { get; init; }
    public string SelectedText { get; init; } = string.Empty;
    public int StartPosition { get; init; }
    public int EndPosition { get; init; }
    public Guid PageId { get; init; }
    public Guid? ChapterId { get; init; }
    public string? ContextBefore { get; init; }
    public string? ContextAfter { get; init; }
    public string? PreferredProvider { get; init; }
    public GenerationParametersDto? Parameters { get; init; }
}

/// <summary>
/// Команда запуска авто веб-новеллы
/// </summary>
public sealed record StartAutoNovelGenerationCommand : IRequest<Result<IReadOnlyList<VisualizationJobSummaryDto>>>
{
    public Guid BookId { get; init; }
    public Guid UserId { get; init; }
    public string? PreferredProvider { get; init; }
    public bool SkipExistingVisualizations { get; init; } = true;
}
