using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Domain.Events;

namespace NovelVision.Services.Catalog.Application.EventHandlers;

public class BookPublishedEventHandler : INotificationHandler<BookPublishedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<BookPublishedEventHandler> _logger;

    public BookPublishedEventHandler(
        IDistributedCache cache,
        ILogger<BookPublishedEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(BookPublishedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Book {BookId} was published at {PublishedAt}",
            notification.BookId.Value,
            notification.PublishedAt);

        // Clear cache for this book
        var cacheKey = $"book:{notification.BookId.Value}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        // Clear published books list cache
        await _cache.RemoveAsync("books:published", cancellationToken);

        // TODO: Trigger visualization generation if enabled
        // TODO: Send notification to subscribers
        // TODO: Update search index
    }
}
