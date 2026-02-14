using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

public interface IVisualizationSettingsService
{
    /// <summary>
    /// Determines which pages/chapters should be visualized based on the mode
    /// </summary>
    Task<Result<VisualizationPlan>> CreateVisualizationPlanAsync(
        BookId bookId,
        VisualizationMode mode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if visualization is allowed for a book
    /// </summary>
    Task<Result<bool>> CanVisualizeAsync(
        BookId bookId,
        CancellationToken cancellationToken = default);
}

public sealed record VisualizationPlan(
    BookId BookId,
    VisualizationMode Mode,
    List<VisualizationTarget> Targets);

public sealed record VisualizationTarget(
    string Id,
    VisualizationTargetType Type,
    string Content,
    int OrderIndex);

public enum VisualizationTargetType
{
    Page,
    Chapter,
    Scene
}
