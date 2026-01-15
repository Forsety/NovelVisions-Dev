// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Events/BookImportedEvent.cs
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Domain.Events;

/// <summary>
/// Событие импорта книги из внешнего источника
/// </summary>
public sealed record BookImportedEvent : DomainEvent
{
    public BookImportedEvent(BookId bookId, ExternalBookId externalId)
    {
        BookId = bookId;
        ExternalId = externalId;
        SourceType = externalId.SourceType;
        ExternalIdValue = externalId.ExternalId;
    }

    public BookId BookId { get; }
    public ExternalBookId ExternalId { get; }
    public ExternalSourceType SourceType { get; }
    public string ExternalIdValue { get; }
}