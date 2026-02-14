namespace NovelVision.BuildingBlocks.SharedKernel.Primitives;

/// <summary>
/// Базовый класс для Aggregate Root с версионированием для optimistic concurrency
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    protected AggregateRoot() : base(default)
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
        Version = 0;
    }

    public long Version { get; private set; }

    public void IncrementVersion()
    {
        Version++;
        UpdateTimestamp();
    }
}

public interface IAggregateRoot : IEntity
{
    long Version { get; }
}

