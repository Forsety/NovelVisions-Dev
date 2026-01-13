// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Repositories/IAuthorRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Repositories;

/// <summary>
/// Репозиторий для работы с авторами
/// </summary>
public interface IAuthorRepository : IRepositoryBase<Author>
{
    /// <summary>
    /// Получить автора по ID
    /// </summary>
    Task<Author?> GetByIdAsync(AuthorId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить автора по email
    /// </summary>
    Task<Author?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование автора
    /// </summary>
    Task<bool> ExistsAsync(AuthorId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить уникальность email
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, AuthorId? excludeAuthorId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить подтвержденных авторов
    /// </summary>
    Task<IReadOnlyList<Author>> GetVerifiedAuthorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить автора по отображаемому имени
    /// </summary>
    Task<Author?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить автора по Gutenberg Author ID
    /// </summary>
    Task<Author?> GetByGutenbergAuthorIdAsync(int gutenbergAuthorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Найти автора по имени из Gutenberg (поиск с нормализацией)
    /// </summary>
    Task<Author?> FindByGutenbergNameAsync(string gutenbergName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Поиск авторов по имени
    /// </summary>
    Task<IReadOnlyList<Author>> SearchByNameAsync(string searchTerm, int maxResults = 10, CancellationToken cancellationToken = default);
}