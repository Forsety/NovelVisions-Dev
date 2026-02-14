// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Repositories/IAuthorRepository.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using System;
namespace NovelVision.Services.Catalog.Domain.Repositories;

/// <summary>
/// ����������� ��� ������ � ��������
/// </summary>
public interface IAuthorRepository : IRepositoryBase<Author>
{
    Task<Author?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    IQueryable<Author> GetQueryable();

    /// <summary>
    /// �������� ������ �� ID
    /// </summary>
    Task<Author?> GetByIdAsync(AuthorId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// �������� ������ �� email
    /// </summary>
    Task<Author?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// ��������� ������������� ������
    /// </summary>
    Task<bool> ExistsAsync(AuthorId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// ��������� ������������ email
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email, AuthorId? excludeAuthorId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// �������� �������������� �������
    /// </summary>
    Task<IReadOnlyList<Author>> GetVerifiedAuthorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// �������� ������ �� ������������� �����
    /// </summary>
    Task<Author?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default);

    /// <summary>
    /// �������� ������ �� Gutenberg Author ID
    /// </summary>
    Task<Author?> GetByGutenbergAuthorIdAsync(int gutenbergAuthorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ����� ������ �� ����� �� Gutenberg (����� � �������������)
    /// </summary>
    Task<Author?> FindByGutenbergNameAsync(string gutenbergName, CancellationToken cancellationToken = default);

    /// <summary>
    /// ����� ������� �� �����
    /// </summary>
    Task<IReadOnlyList<Author>> SearchByNameAsync(string searchTerm, int maxResults = 10, CancellationToken cancellationToken = default);
}