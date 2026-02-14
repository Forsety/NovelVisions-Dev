// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Events/ChapterReorderedEvent.cs
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Events;

/// <summary>
/// Событие: глава переупорядочена в книге
/// </summary>
public sealed record ChapterReorderedEvent(
    BookId BookId,
    ChapterId ChapterId,
    int OldOrderIndex,
    int NewOrderIndex) : DomainEvent;