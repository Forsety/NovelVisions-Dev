// =============================================================================
// ФАЙЛ: src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Repositories/ISubjectRepository.cs
// ДЕЙСТВИЕ: ДОБАВИТЬ метод GetByNameAsync в интерфейс
// ПРИЧИНА: Метод используется в коде, но не определён в интерфейсе
// =============================================================================

using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Repositories;

/// <summary>
/// Репозиторий для работы с категориями/темами
/// </summary>
public interface ISubjectRepository
{
    /// <summary>
    /// Получает категорию по ID
    /// </summary>
    Task<Subject?> GetByIdAsync(SubjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает категорию по имени и типу
    /// </summary>
    Task<Subject?> GetByNameAsync(string name, SubjectType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает категорию по slug
    /// </summary>
    Task<Subject?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все категории указанного типа
    /// </summary>
    Task<IReadOnlyList<Subject>> GetByTypeAsync(SubjectType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает дочерние категории
    /// </summary>
    Task<IReadOnlyList<Subject>> GetChildrenAsync(SubjectId parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает корневые категории (без родителя)
    /// </summary>
    Task<IReadOnlyList<Subject>> GetRootSubjectsAsync(SubjectType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет существование категории
    /// </summary>
    Task<bool> ExistsAsync(SubjectId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет уникальность slug
    /// </summary>
    Task<bool> IsSlugUniqueAsync(string slug, SubjectId? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавляет категорию
    /// </summary>
    Task AddAsync(Subject subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет категорию
    /// </summary>
    Task UpdateAsync(Subject subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет категорию
    /// </summary>
    Task DeleteAsync(Subject subject, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает или создаёт категорию по имени
    /// </summary>
    Task<Subject> GetOrCreateAsync(string name, SubjectType type, CancellationToken cancellationToken = default);
}