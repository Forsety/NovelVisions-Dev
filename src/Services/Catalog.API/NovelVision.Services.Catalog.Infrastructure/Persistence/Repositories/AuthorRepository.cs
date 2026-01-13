// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Repositories/AuthorRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Repositories;

public class AuthorRepository : RepositoryBase<Author>, IAuthorRepository
{
    private readonly CatalogDbContext _dbContext;

    public AuthorRepository(CatalogDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Author?> GetByIdAsync(AuthorId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Author?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExistsAsync(AuthorId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, AuthorId? excludeAuthorId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Authors
            .Where(a => a.Email == email.ToLowerInvariant());

        if (excludeAuthorId != null)
        {
            query = query.Where(a => a.Id != excludeAuthorId);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Author>> GetVerifiedAuthorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .Where(a => a.IsVerified)
            .OrderBy(a => a.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public override async Task<Author> AddAsync(Author entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Authors.AddAsync(entity, cancellationToken);
        return entity;
    }

    public override async Task UpdateAsync(Author entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Authors.Update(entity);
        await Task.CompletedTask;
    }

    public override async Task DeleteAsync(Author entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Authors.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<Author?> GetByDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.DisplayName == displayName, cancellationToken);
    }

    public async Task<Author?> GetByGutenbergAuthorIdAsync(int gutenbergAuthorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.ExternalIds != null && a.ExternalIds.GutenbergAuthorId == gutenbergAuthorId, cancellationToken);
    }

    public async Task<Author?> FindByGutenbergNameAsync(string gutenbergName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gutenbergName))
            return null;

        // Нормализуем имя для поиска
        var normalizedName = NormalizeName(gutenbergName);

        // Сначала пробуем точное совпадение
        var author = await _dbContext.Authors
            .FirstOrDefaultAsync(a => a.DisplayName == gutenbergName, cancellationToken);

        if (author != null)
            return author;

        // Пробуем поиск по нормализованному имени
        var authors = await _dbContext.Authors
            .ToListAsync(cancellationToken);

        return authors.FirstOrDefault(a =>
            NormalizeName(a.DisplayName).Equals(normalizedName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<Author>> SearchByNameAsync(string searchTerm, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Author>();

        var term = searchTerm.ToLowerInvariant();

        return await _dbContext.Authors
            .Where(a => a.DisplayName.ToLower().Contains(term))
            .OrderBy(a => a.DisplayName)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Нормализует имя автора для поиска (удаляет "Last, First" формат и т.д.)
    /// </summary>
    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Удаляем даты жизни если есть (формат "Name, 1800-1850")
        var parts = name.Split(',');
        if (parts.Length > 1)
        {
            // Проверяем, является ли последняя часть датами
            var lastPart = parts[^1].Trim();
            if (lastPart.Any(char.IsDigit))
            {
                // Убираем даты
                name = string.Join(",", parts.Take(parts.Length - 1));
            }
        }

        // Преобразуем "Last, First" в "First Last"
        parts = name.Split(',');
        if (parts.Length == 2)
        {
            name = $"{parts[1].Trim()} {parts[0].Trim()}";
        }

        return name.Trim();
    }
}