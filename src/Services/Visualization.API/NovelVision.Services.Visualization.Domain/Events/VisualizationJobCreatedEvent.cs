using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Domain.Events;

/// <summary>
/// Domain event raised when a new visualization job is created
/// </summary>
public sealed record VisualizationJobCreatedEvent(
    VisualizationJobId JobId,
    BookId BookId,
    UserId UserId) : DomainEvent;

/// <summary>
/// Domain event raised when a visualization job is successfully completed
/// </summary>
public sealed record VisualizationJobCompletedEvent(
    VisualizationJobId JobId,
    BookId BookId,
    string ImageUrl) : DomainEvent;

/// <summary>
/// Domain event raised when a visualization job fails
/// </summary>
public sealed record VisualizationJobFailedEvent(
    VisualizationJobId JobId,
    BookId BookId,
    string Error) : DomainEvent;