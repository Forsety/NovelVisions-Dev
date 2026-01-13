
using  NovelVision.BuildingBlocks.SharedKernel.Primitives;
using Ardalis.Specification;

namespace NovelVision.BuildingBlocks.SharedKernel.Repositories;

/// <summary>
/// Базовый интерфейс репозитория с поддержкой спецификаций
/// </summary>
public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
}

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot
{
}