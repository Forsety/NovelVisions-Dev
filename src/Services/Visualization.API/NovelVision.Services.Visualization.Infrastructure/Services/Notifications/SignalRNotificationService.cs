// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Notifications/SignalRNotificationService.cs

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Infrastructure.Hubs;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Notifications;

/// <summary>
/// SignalR реализация сервиса уведомлений
/// </summary>
public sealed class SignalRNotificationService : IVisualizationNotificationService
{
    private readonly IHubContext<VisualizationHub, IVisualizationHubClient> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<VisualizationHub, IVisualizationHubClient> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyJobProgressAsync(
        Guid userId,
        JobProgressDto progress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = GetUserGroup(userId);
            await _hubContext.Clients.Group(groupName)
                .OnJobProgress(progress);

            _logger.LogDebug(
                "Sent progress notification for job {JobId} to user {UserId}: {Status} ({Progress}%)",
                progress.JobId, userId, progress.Status, progress.ProgressPercent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send progress notification for job {JobId} to user {UserId}",
                progress.JobId, userId);
        }
    }

    public async Task NotifyJobCompletedAsync(
        Guid userId,
        VisualizationJobDto job,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = GetUserGroup(userId);
            await _hubContext.Clients.Group(groupName)
                .OnJobCompleted(job);

            _logger.LogInformation(
                "Sent completion notification for job {JobId} to user {UserId}",
                job.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send completion notification for job {JobId} to user {UserId}",
                job.Id, userId);
        }
    }

    public async Task NotifyJobFailedAsync(
        Guid userId,
        Guid jobId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = GetUserGroup(userId);
            await _hubContext.Clients.Group(groupName)
                .OnJobFailed(jobId, errorMessage);

            _logger.LogInformation(
                "Sent failure notification for job {JobId} to user {UserId}: {Error}",
                jobId, userId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send failure notification for job {JobId} to user {UserId}",
                jobId, userId);
        }
    }

    public async Task NotifyQueueUpdateAsync(
        Guid userId,
        QueueStatusDto queueStatus,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = GetUserGroup(userId);
            await _hubContext.Clients.Group(groupName)
                .OnQueueUpdate(queueStatus);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to send queue update to user {UserId}", userId);
        }
    }

    private static string GetUserGroup(Guid userId) => $"user_{userId}";
}