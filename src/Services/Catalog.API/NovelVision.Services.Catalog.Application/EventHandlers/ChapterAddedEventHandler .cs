using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Domain.Events;

namespace NovelVision.Services.Catalog.Application.EventHandlers;

public class ChapterAddedEventHandler : INotificationHandler<ChapterAddedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ChapterAddedEventHandler> _logger;

    public ChapterAddedEventHandler(
        IDistributedCache cache,
        ILogger<ChapterAddedEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(ChapterAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Chapter {ChapterId} added to Book {BookId}",
            notification.ChapterId.Value,
            notification.BookId.Value);

        // Clear book cache
        var cacheKey = $"book:{notification.BookId.Value}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        // TODO: Update book statistics
        // TODO: Trigger auto-save if enabled
    }
}
