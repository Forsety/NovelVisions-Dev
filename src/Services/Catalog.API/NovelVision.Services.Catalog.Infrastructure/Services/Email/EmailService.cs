using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Application.Common.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NovelVision.Services.Catalog.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ISendGridClient sendGridClient,
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _sendGridClient = sendGridClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var from = new EmailAddress(
                _configuration["Email:FromAddress"],
                _configuration["Email:FromName"]);

            var toAddress = new EmailAddress(to);

            var msg = MailHelper.CreateSingleEmail(
                from,
                toAddress,
                subject,
                isHtml ? null : body,
                isHtml ? body : null);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                _logger.LogError("Failed to send email to {To}. Status: {Status}",
                    to, response.StatusCode);
            }
            else
            {
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
            throw;
        }
    }

    public async Task SendBookPublishedNotificationAsync(
        string authorEmail,
        string bookTitle,
        CancellationToken cancellationToken = default)
    {
        var subject = "Your Book Has Been Published!";
        var body = $@"
            <html>
            <body>
                <h2>Congratulations!</h2>
                <p>Your book '<strong>{bookTitle}</strong>' has been successfully published on NovelVision platform.</p>
                <p>Readers can now discover and enjoy your work.</p>
                <br/>
                <p>Best regards,<br/>NovelVision Team</p>
            </body>
            </html>";

        await SendEmailAsync(authorEmail, subject, body, true, cancellationToken);
    }

    public async Task SendAuthorVerifiedNotificationAsync(
        string authorEmail,
        string authorName,
        CancellationToken cancellationToken = default)
    {
        var subject = "Your Author Account Has Been Verified!";
        var body = $@"
            <html>
            <body>
                <h2>Welcome, {authorName}!</h2>
                <p>Your author account has been successfully verified.</p>
                <p>You can now:</p>
                <ul>
                    <li>Publish your books</li>
                    <li>Access advanced author features</li>
                    <li>Enable AI visualization for your books</li>
                </ul>
                <br/>
                <p>Happy writing!<br/>NovelVision Team</p>
            </body>
            </html>";

        await SendEmailAsync(authorEmail, subject, body, true, cancellationToken);
    }
}
