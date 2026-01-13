// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Persistence/Repositories/PageRepository.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для страниц
/// </summary>
public class PageRepository : IPageRepository
{
    private readonly CatalogDbContext _context;

    public PageRepository(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task<Page?> GetByIdAsync(PageId id, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Page>> GetByChapterIdAsync(ChapterId chapterId, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .Where(p => p.ChapterId == chapterId)
            .OrderBy(p => p.PageNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Page>> GetVisualizationPointsAsync(BookId bookId, CancellationToken cancellationToken = default)
    {
        // Получаем все главы книги
        var chapterIds = await _context.Chapters
            .Where(c => c.BookId == bookId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        // Получаем страницы с пометкой визуализации
        return await _context.Pages
            .Where(p => chapterIds.Contains(p.ChapterId) && p.IsVisualizationPoint)
            .OrderBy(p => p.ChapterId)
            .ThenBy(p => p.PageNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Page>> GetPagesWithoutVisualizationAsync(ChapterId chapterId, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .Where(p => p.ChapterId == chapterId && p.VisualizationImageUrl == null)
            .OrderBy(p => p.PageNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Page>> GetPagesWithVisualizationAsync(BookId bookId, CancellationToken cancellationToken = default)
    {
        var chapterIds = await _context.Chapters
            .Where(c => c.BookId == bookId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return await _context.Pages
            .Where(p => chapterIds.Contains(p.ChapterId) && p.VisualizationImageUrl != null)
            .OrderBy(p => p.ChapterId)
            .ThenBy(p => p.PageNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Page?> GetByChapterAndNumberAsync(ChapterId chapterId, int pageNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .FirstOrDefaultAsync(p => p.ChapterId == chapterId && p.PageNumber == pageNumber, cancellationToken);
    }

    public async Task<int> GetPageCountByChapterAsync(ChapterId chapterId, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .CountAsync(p => p.ChapterId == chapterId, cancellationToken);
    }

    public async Task UpdateAsync(Page page, CancellationToken cancellationToken = default)
    {
        _context.Pages.Update(page);
        await Task.CompletedTask;
    }

    public async Task AddAsync(Page page, CancellationToken cancellationToken = default)
    {
        await _context.Pages.AddAsync(page, cancellationToken);
    }

    public async Task DeleteAsync(Page page, CancellationToken cancellationToken = default)
    {
        _context.Pages.Remove(page);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(PageId id, CancellationToken cancellationToken = default)
    {
        return await _context.Pages.AnyAsync(p => p.Id == id, cancellationToken);
    }
}