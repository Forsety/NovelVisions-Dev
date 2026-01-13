using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Services;

/// <summary>
/// Domain service for complex business logic that spans multiple aggregates
/// </summary>
public interface IBookDomainService
{
    /// <summary>
    /// Transfers a book from one author to another
    /// </summary>
    Task<Result<bool>> TransferBookOwnershipAsync(
        BookId bookId,
        AuthorId fromAuthorId,
        AuthorId toAuthorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a book can be published based on business rules
    /// </summary>
    Task<Result<bool>> ValidateForPublishingAsync(
        Book book,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates book statistics and metrics
    /// </summary>
    Task<BookStatistics> CalculateStatisticsAsync(
        BookId bookId,
        CancellationToken cancellationToken = default);
}
