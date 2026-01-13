// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Events/AuthorEvents.cs
using System;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Events;

public sealed record AuthorCreatedEvent(
    AuthorId AuthorId,
    string DisplayName,
    string Email) : DomainEvent;

public sealed record AuthorProfileUpdatedEvent(
    AuthorId AuthorId,
    string DisplayName,
    string? Biography) : DomainEvent;

public sealed record AuthorVerifiedEvent : DomainEvent
{
    public AuthorId AuthorId { get; }
    public DateTime VerifiedAt { get; }

    public AuthorVerifiedEvent(AuthorId authorId)
    {
        AuthorId = authorId;
        VerifiedAt = DateTime.UtcNow;
    }

    public AuthorVerifiedEvent(AuthorId authorId, DateTime verifiedAt)
    {
        AuthorId = authorId;
        VerifiedAt = verifiedAt;
    }
}

public sealed record AuthorEmailChangedEvent(
    AuthorId AuthorId,
    string NewEmail) : DomainEvent;

public sealed record BookAddedToAuthorEvent(
    AuthorId AuthorId,
    BookId BookId) : DomainEvent;

public sealed record BookRemovedFromAuthorEvent(
    AuthorId AuthorId,
    BookId BookId) : DomainEvent;

/// <summary>
/// Событие связывания автора с пользователем
/// </summary>
public sealed record AuthorLinkedToUserEvent(
    AuthorId AuthorId,
    string UserId) : DomainEvent;

/// <summary>
/// Событие отвязки автора от пользователя
/// </summary>
public sealed record AuthorUnlinkedFromUserEvent(
    AuthorId AuthorId,
    string UserId) : DomainEvent;