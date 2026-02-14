using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Repositories;

public interface IBookRepository : IRepository<Book>
{
    Task<Book?> GetByIdAsync(BookId id, CancellationToken cancellationToken = default);
    Task<Book?> GetByISBNAsync(string isbn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetByAuthorAsync(AuthorId authorId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(BookId id, CancellationToken cancellationToken = default);
    Task<bool> IsISBNUniqueAsync(string isbn, BookId? excludeBookId = null, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdWithChaptersAndPagesAsync(BookId id, CancellationToken cancellationToken = default);
    Task<Book?> GetByIdWithChaptersAsync(BookId id, CancellationToken cancellationToken = default);
    Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByExternalIdAsync(ExternalSourceType sourceType, string externalId, CancellationToken cancellationToken = default);
    Task<List<Book>> GetBySourceAsync(BookSource source, CancellationToken cancellationToken = default);

    Task<(List<Book> Books, int TotalCount)> SearchAdvancedAsync(
        string? searchTerm,
        List<string>? genres,
        List<string>? languages,
        int? minPageCount,
        int? maxPageCount,
        int pageNumber,
        int pageSize,
        string? sortBy,
        bool descending,
        CancellationToken cancellationToken = default);
}