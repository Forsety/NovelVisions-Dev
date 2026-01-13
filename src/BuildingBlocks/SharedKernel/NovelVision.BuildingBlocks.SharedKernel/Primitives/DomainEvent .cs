namespace NovelVision.BuildingBlocks.SharedKernel.Primitives;

/// <summary>
/// Базовая реализация доменного события
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid EventId { get; private init; }
    public DateTime OccurredOn { get; private init; }
}
