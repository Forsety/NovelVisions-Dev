using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Domain.Events;

namespace NovelVision.Services.Catalog.Application.EventHandlers;

public class AuthorVerifiedEventHandler : INotificationHandler<AuthorVerifiedEvent>
{
    private readonly ILogger<AuthorVerifiedEventHandler> _logger;
    // TODO: Inject email service

    public AuthorVerifiedEventHandler(ILogger<AuthorVerifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AuthorVerifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Author {AuthorId} verified at {VerifiedAt}",
            notification.AuthorId.Value,
            notification.VerifiedAt);

        // TODO: Send congratulations email
        // TODO: Grant verified author permissions
        // TODO: Update author badge/status in UI
        
        await Task.CompletedTask;
    }
}
