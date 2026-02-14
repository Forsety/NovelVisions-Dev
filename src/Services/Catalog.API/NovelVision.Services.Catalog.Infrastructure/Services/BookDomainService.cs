using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.Rules;
using NovelVision.Services.Catalog.Domain.Services;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Services;

public class BookDomainService : IBookDomainService
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ILogger<BookDomainService> _logger;

    public BookDomainService(
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        ILogger<BookDomainService> logger)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> TransferBookOwnershipAsync(
        BookId bookId,
        AuthorId fromAuthorId,
        AuthorId toAuthorId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transferring book {BookId} from author {FromAuthorId} to {ToAuthorId}",
            bookId.Value, fromAuthorId.Value, toAuthorId.Value);

        // Get the book
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Book {bookId.Value} not found"));
        }

        // Verify current ownership
        if (book.AuthorId != fromAuthorId)
        {
            return Result<bool>.Failure(Error.Validation($"Book does not belong to author {fromAuthorId.Value}"));
        }

        // Verify new author exists
        var newAuthor = await _authorRepository.GetByIdAsync(toAuthorId, cancellationToken);
        if (newAuthor == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Author {toAuthorId.Value} not found"));
        }

        // Get old author
        var oldAuthor = await _authorRepository.GetByIdAsync(fromAuthorId, cancellationToken);
        if (oldAuthor != null)
        {
            oldAuthor.RemoveBook(bookId);
            await _authorRepository.UpdateAsync(oldAuthor, cancellationToken);
        }

        // Update book ownership (need to use reflection as AuthorId is readonly)
        var authorIdProperty = book.GetType().GetProperty(nameof(Book.AuthorId));
        authorIdProperty?.SetValue(book, toAuthorId);

        // Update new author
        newAuthor.AddBook(bookId);

        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _authorRepository.UpdateAsync(newAuthor, cancellationToken);

        _logger.LogInformation("Successfully transferred book {BookId} ownership", bookId.Value);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ValidateForPublishingAsync(
        Book book,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // Check minimum pages
        if (book.TotalPageCount < BookBusinessRules.MinPagesForPublishing)
        {
            errors.Add($"Book must have at least {BookBusinessRules.MinPagesForPublishing} pages");
        }

        // Check chapters
        if (book.ChapterCount == 0)
        {
            errors.Add("Book must have at least one chapter");
        }

        // Check if all chapters have pages
        if (book.Chapters.Any(c => c.PageCount == 0))
        {
            errors.Add("All chapters must have at least one page");
        }

        // Check author verification
        var author = await _authorRepository.GetByIdAsync(book.AuthorId, cancellationToken);
        if (author != null && !author.IsVerified)
        {
            errors.Add("Author must be verified to publish books");
        }

        if (errors.Any())
        {
            return Result<bool>.Failure(Error.Validation(string.Join("; ", errors)));
        }

        return Result<bool>.Success(true);
    }

    public async Task<BookStatistics> CalculateStatisticsAsync(
        BookId bookId,
        CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book == null)
        {
            throw new InvalidOperationException($"Book {bookId.Value} not found");
        }

        var chapterWordCounts = book.Chapters
            .ToDictionary(c => c.Title, c => c.TotalWordCount);

        var totalWords = book.TotalWordCount;
        var totalPages = book.TotalPageCount;
        var totalChapters = book.ChapterCount;

        return new BookStatistics(
            ChapterCount: totalChapters,
            PageCount: totalPages,
            WordCount: totalWords,
            EstimatedReadingTime: book.EstimatedReadingTime,
            ChapterWordCounts: chapterWordCounts,
            AverageWordsPerPage: totalPages > 0 ? (double)totalWords / totalPages : 0,
            AverageWordsPerChapter: totalChapters > 0 ? (double)totalWords / totalChapters : 0
        );
    }
}
