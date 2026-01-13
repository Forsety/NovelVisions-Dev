// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Repositories/SubjectRepository.cs
// ИСПРАВЛЕНИЕ: Полная реализация ISubjectRepository
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Infrastructure.Persistence;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторий для работы с категориями/темами
/// </summary>
public class SubjectRepository : ISubjectRepository
{
    private readonly CatalogDbContext _dbContext;

    public SubjectRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<Subject?> GetByIdAsync(SubjectId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Subject?> GetByNameAsync(string name, SubjectType type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name.Trim().ToLowerInvariant();

        return await _dbContext.Subjects
            .FirstOrDefaultAsync(s =>
                s.Name.ToLower() == normalizedName &&
                s.Type == type,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Subject?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        return await _dbContext.Subjects
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Subject>> GetByTypeAsync(SubjectType type, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .Where(s => s.Type == type)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Subject>> GetChildrenAsync(SubjectId parentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .Where(s => s.ParentId == parentId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Subject>> GetRootSubjectsAsync(SubjectType? type = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Subjects
            .Where(s => s.ParentId == null);

        if (type is not null)
        {
            query = query.Where(s => s.Type == type.Value);
        }

        return await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(SubjectId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .AnyAsync(s => s.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> IsSlugUniqueAsync(string slug, SubjectId? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        var query = _dbContext.Subjects.Where(s => s.Slug == slug);

        if (excludeId is not null)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        if (subject == null)
            throw new ArgumentNullException(nameof(subject));

        await _dbContext.Subjects.AddAsync(subject, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        if (subject == null)
            throw new ArgumentNullException(nameof(subject));

        _dbContext.Subjects.Update(subject);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync(Subject subject, CancellationToken cancellationToken = default)
    {
        if (subject == null)
            throw new ArgumentNullException(nameof(subject));

        _dbContext.Subjects.Remove(subject);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Subject> GetOrCreateAsync(string name, SubjectType type, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        // Try to find existing
        var existing = await GetByNameAsync(name, type, cancellationToken);
        if (existing != null)
            return existing;

        // Create new subject
        var subject = Subject.Create(name, type, externalMapping: name);
        await AddAsync(subject, cancellationToken);

        return subject;
    }

    /// <summary>
    /// Получает категории по списку ID
    /// </summary>
    public async Task<IReadOnlyList<Subject>> GetByIdsAsync(
        IEnumerable<SubjectId> ids,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (!idList.Any())
            return new List<Subject>();

        return await _dbContext.Subjects
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Поиск категорий по названию
    /// </summary>
    public async Task<IReadOnlyList<Subject>> SearchAsync(
        string searchTerm,
        SubjectType? type = null,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Subject>();

        var normalizedTerm = searchTerm.Trim().ToLowerInvariant();

        var query = _dbContext.Subjects
            .Where(s => s.Name.ToLower().Contains(normalizedTerm));

        if (type is not null)
        {
            query = query.Where(s => s.Type == type.Value);
        }

        return await query
            .OrderBy(s => s.Name)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Получает популярные категории по количеству книг
    /// </summary>
    public async Task<IReadOnlyList<Subject>> GetPopularAsync(
        int count = 10,
        SubjectType? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Subjects.AsQueryable();

        if (type is not null)
        {
            query = query.Where(s => s.Type == type.Value);
        }

        return await query
            .OrderByDescending(s => s.BookCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}