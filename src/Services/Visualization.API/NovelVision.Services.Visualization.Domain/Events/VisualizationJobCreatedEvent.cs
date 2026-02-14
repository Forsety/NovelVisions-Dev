using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.Events;

/// <summary>
/// Domain event raised when a new visualization job is created
/// </summary>
public sealed record VisualizationJobCreatedEvent(
    Guid VisualizationJobId,
    Guid BookId,
    Guid UserId
) : DomainEvent;

/// <summary>
/// Domain event raised when a visualization job is successfully completed
/// </summary>
public sealed record VisualizationJobCompletedEvent(
    VisualizationJobId JobId,
    Guid BookId,
    string ImageUrl) : DomainEvent;

/// <summary>
/// Domain event raised when a visualization job fails
/// </summary>
public sealed record VisualizationJobFailedEvent(
    VisualizationJobId JobId,
   Guid BookId,
    string Error) : DomainEvent;