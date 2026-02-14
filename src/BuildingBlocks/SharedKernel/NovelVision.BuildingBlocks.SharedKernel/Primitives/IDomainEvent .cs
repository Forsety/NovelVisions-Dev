// src/BuildingBlocks/SharedKernel/NovelVision.BuildingBlocks.SharedKernel/Primitives/IDomainEvent.cs
// ИСПРАВЛЕНИЕ: Удалён повреждённый код с индексатором
using System;
using MediatR;

namespace NovelVision.BuildingBlocks.SharedKernel.Primitives;

/// <summary>
/// Маркерный интерфейс для доменных событий
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Дата и время возникновения события
    /// </summary>
    DateTime OccurredOn { get; }
}