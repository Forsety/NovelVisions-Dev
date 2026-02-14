using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Application.Common.Interfaces;

namespace NovelVision.Services.Catalog.Infrastructure.Services.Email;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Email - To: {To}, Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendBookPublishedNotificationAsync(
        string authorEmail,
        string bookTitle,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Email - Book Published: {BookTitle} to {Email}", bookTitle, authorEmail);
        return Task.CompletedTask;
    }

    public Task SendAuthorVerifiedNotificationAsync(
        string authorEmail,
        string authorName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock Email - Author Verified: {AuthorName} to {Email}", authorName, authorEmail);
        return Task.CompletedTask;
    }
}
