// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Events/ChaptersRemovedEvent.cs
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Events;

/// <summary>
/// Событие об удалении всех глав книги (при реимпорте)
/// </summary>
public sealed record ChaptersRemovedEvent : DomainEvent
{
    /// <summary>
    /// ID книги
    /// </summary>
    public BookId BookId { get; }

    /// <summary>
    /// Количество удалённых глав
    /// </summary>
    public int RemovedCount { get; }

    public ChaptersRemovedEvent(BookId bookId, int removedCount)
    {
        BookId = bookId;
        RemovedCount = removedCount;
    }
}