using System.Threading;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default);

    Task SendBookPublishedNotificationAsync(
        string authorEmail,
        string bookTitle,
        CancellationToken cancellationToken = default);

    Task SendAuthorVerifiedNotificationAsync(
        string authorEmail,
        string authorName,
        CancellationToken cancellationToken = default);
}
