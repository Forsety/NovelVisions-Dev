// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Repositories/BookRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Repositories;

public class BookRepository : RepositoryBase<Book>, IBookRepository
{
    private readonly CatalogDbContext _dbContext;

    public BookRepository(CatalogDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Include(b => b.Chapters)
                .ThenInclude(c => c.Pages)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Where(b => b.ISBN != null && b.ISBN.Value == isbn)
            .Include(b => b.Chapters)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetByAuthorAsync(AuthorId authorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Where(b => b.AuthorId == authorId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(BookId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .AnyAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<bool> IsISBNUniqueAsync(string isbn, BookId? excludeBookId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Books
            .Where(b => b.ISBN != null && b.ISBN.Value == isbn);

        if (excludeBookId != null)
        {
            query = query.Where(b => b.Id != excludeBookId);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public override async Task<Book> AddAsync(Book entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Books.AddAsync(entity, cancellationToken);
        return entity;
    }

    public override async Task UpdateAsync(Book entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Books.Update(entity);
        await Task.CompletedTask;
    }

    public override async Task DeleteAsync(Book entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Books.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<Book?> GetByIdWithChaptersAsync(
        BookId id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Include(b => b.Chapters)
                .ThenInclude(c => c.Pages)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByGutenbergIdAsync(int gutenbergId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Include(b => b.Chapters)
                .ThenInclude(c => c.Pages)
            .FirstOrDefaultAsync(b => b.ExternalIds != null && b.ExternalIds.GutenbergId == gutenbergId, cancellationToken);
    }

    public async Task<List<Book>> GetBySubjectAsync(SubjectId subjectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Where(b => b.SubjectIds.Any(id => id == subjectId))
            .OrderByDescending(b => b.Statistics.DownloadCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Book>> GetPopularAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Where(b => b.Status == BookStatus.Published)
            .OrderByDescending(b => b.Statistics.DownloadCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Book>> GetBySourceAsync(BookSource source, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books
            .Where(b => b.Source == source)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByExternalIdAsync(
    ExternalSourceType sourceType,
    string externalId,
    CancellationToken cancellationToken = default)
    {
        if (sourceType == ExternalSourceType.Gutenberg)
        {
            if (int.TryParse(externalId, out var gutenbergId))
            {
                return await _dbContext.Books.AnyAsync(
                    b => b.ExternalIds != null && b.ExternalIds.GutenbergId == gutenbergId,
                    cancellationToken);
            }
            return false;
        }

        if (sourceType == ExternalSourceType.OpenLibrary)
        {
            return await _dbContext.Books.AnyAsync(
                b => b.ExternalIds != null &&
                     (b.ExternalIds.OpenLibraryWorkId == externalId ||
                      b.ExternalIds.OpenLibraryEditionId == externalId),
                cancellationToken);
        }

        return false;
    }

    public async Task<(List<Book> Books, int TotalCount)> SearchAdvancedAsync(
        string? searchTerm,
        List<SubjectId>? subjectIds,
        List<string>? genres,
        List<string>? languages,
        CopyrightStatus? copyrightStatus,
        BookSource? source,
        int? minPageCount,
        int? maxPageCount,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Books.AsQueryable();

        // Search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(b =>
                b.Metadata.Title.ToLower().Contains(term) ||
                (b.Metadata.Description != null && b.Metadata.Description.ToLower().Contains(term)));
        }

        // Subject filter
        if (subjectIds is not null && subjectIds.Any())
        {
            query = query.Where(b => b.SubjectIds.Any(sid => subjectIds.Contains(sid)));
        }

        // Genre filter
        if (genres is not null && genres.Any())
        {
            query = query.Where(b => b.Genres.Any(g => genres.Contains(g)));
        }

        // Copyright filter
        if (copyrightStatus is not null)
        {
            var status = copyrightStatus.Value;
            query = query.Where(b => b.CopyrightStatus == status);
        }

        // Source filter
        if (source is not null)
        {
            var src = source.Value;
            query = query.Where(b => b.Source == src);
        }

        // Page count filters
        if (minPageCount.HasValue)
        {
            query = query.Where(b => b.Metadata.PageCount >= minPageCount.Value);
        }
        if (maxPageCount.HasValue)
        {
            query = query.Where(b => b.Metadata.PageCount <= maxPageCount.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        query = sortBy?.ToLower() switch
        {
            "title" => descending
                ? query.OrderByDescending(b => b.Metadata.Title)
                : query.OrderBy(b => b.Metadata.Title),
            "downloads" => descending
                ? query.OrderByDescending(b => b.Statistics.DownloadCount)
                : query.OrderBy(b => b.Statistics.DownloadCount),
            "created" => descending
                ? query.OrderByDescending(b => b.CreatedAt)
                : query.OrderBy(b => b.CreatedAt),
            "pages" => descending
                ? query.OrderByDescending(b => b.Metadata.PageCount)
                : query.OrderBy(b => b.Metadata.PageCount),
            _ => query.OrderByDescending(b => b.Statistics.DownloadCount)
        };

        // Pagination
        var books = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (books, totalCount);
    }
}