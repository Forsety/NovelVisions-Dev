namespace NovelVision.BuildingBlocks.SharedKernel.Primitives;

/// <summary>
/// Маркерный интерфейс для всех сущностей
/// </summary>
public interface IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
