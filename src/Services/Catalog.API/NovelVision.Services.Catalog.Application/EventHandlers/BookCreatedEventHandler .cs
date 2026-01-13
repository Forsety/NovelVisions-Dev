using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Domain.Events;
using NovelVision.Services.Catalog.Domain.Repositories;

namespace NovelVision.Services.Catalog.Application.EventHandlers;

public class BookCreatedEventHandler : INotificationHandler<BookCreatedEvent>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ILogger<BookCreatedEventHandler> _logger;

    public BookCreatedEventHandler(
        IAuthorRepository authorRepository,
        ILogger<BookCreatedEventHandler> logger)
    {
        _authorRepository = authorRepository;
        _logger = logger;
    }

    public async Task Handle(BookCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling BookCreatedEvent for Book {BookId} by Author {AuthorId}",
            notification.BookId.Value,
            notification.AuthorId.Value);

        // Update author's book count
        var author = await _authorRepository.GetByIdAsync(notification.AuthorId, cancellationToken);
        if (author != null)
        {
            author.AddBook(notification.BookId);
            await _authorRepository.UpdateAsync(author, cancellationToken);
        }

        // TODO: Send notification email to author
        // TODO: Update search index
        // TODO: Clear relevant caches
    }
}
