// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Events/PageEvents.cs
using System;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Events;

/// <summary>
/// Событие: визуализация установлена на страницу
/// </summary>
public sealed record PageVisualizationSetEvent : DomainEvent
{
    public PageVisualizationSetEvent(
        PageId pageId,
        ChapterId chapterId,
        string imageUrl,
        string? thumbnailUrl = null,
        Guid? jobId = null)
    {
        PageId = pageId;
        ChapterId = chapterId;
        ImageUrl = imageUrl;
        ThumbnailUrl = thumbnailUrl;
        JobId = jobId;
    }

    public PageId PageId { get; }
    public ChapterId ChapterId { get; }
    public string ImageUrl { get; }
    public string? ThumbnailUrl { get; }
    public Guid? JobId { get; }
}

/// <summary>
/// Событие: визуализация удалена со страницы
/// </summary>
public sealed record PageVisualizationRemovedEvent(
    PageId PageId,
    ChapterId ChapterId) : DomainEvent;

/// <summary>
/// Событие: визуализация очищена со страницы
/// </summary>
public sealed record PageVisualizationClearedEvent(
    PageId PageId,
    ChapterId ChapterId) : DomainEvent;

/// <summary>
/// Событие: генерация визуализации запущена
/// </summary>
public sealed record PageVisualizationStartedEvent(
    PageId PageId,
    ChapterId ChapterId,
    Guid JobId) : DomainEvent;

/// <summary>
/// Событие: генерация визуализации провалилась
/// </summary>
public sealed record PageVisualizationFailedEvent(
    PageId PageId,
    ChapterId ChapterId,
    string? ErrorMessage) : DomainEvent;

/// <summary>
/// Событие: страница помечена как точка визуализации автором
/// </summary>
public sealed record PageMarkedAsVisualizationPointEvent(
    PageId PageId,
    ChapterId ChapterId,
    string? AuthorHint) : DomainEvent;

/// <summary>
/// Событие: пометка точки визуализации снята со страницы
/// </summary>
public sealed record PageVisualizationPointUnmarkedEvent(
    PageId PageId,
    ChapterId ChapterId) : DomainEvent;

/// <summary>
/// Событие: контент страницы обновлен
/// </summary>
public sealed record PageContentUpdatedEvent(
    PageId PageId,
    ChapterId ChapterId,
    int OldWordCount,
    int NewWordCount) : DomainEvent;

/// <summary>
/// Событие: авторская подсказка для визуализации обновлена
/// </summary>
public sealed record PageVisualizationHintUpdatedEvent(
    PageId PageId,
    ChapterId ChapterId,
    string? NewHint) : DomainEvent;

/// <summary>
/// Событие: страница создана
/// </summary>
public sealed record PageCreatedEvent(
    PageId PageId,
    ChapterId ChapterId,
    int PageNumber) : DomainEvent;

/// <summary>
/// Событие: страница удалена
/// </summary>
public sealed record PageDeletedEvent(
    PageId PageId,
    ChapterId ChapterId) : DomainEvent;

/// <summary>
/// Событие: номер страницы изменён
/// </summary>
public sealed record PageNumberChangedEvent(
    PageId PageId,
    ChapterId ChapterId,
    int OldPageNumber,
    int NewPageNumber) : DomainEvent;