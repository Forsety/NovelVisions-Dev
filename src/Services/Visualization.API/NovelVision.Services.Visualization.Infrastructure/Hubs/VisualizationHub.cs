// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Hubs/VisualizationHub.cs

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Visualization.Application.DTOs;

namespace NovelVision.Services.Visualization.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub для real-time уведомлений о визуализации
/// </summary>
[Authorize]
public sealed class VisualizationHub : Hub<IVisualizationHubClient>
{
    private readonly ILogger<VisualizationHub> _logger;

    public VisualizationHub(ILogger<VisualizationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            // Добавляем пользователя в его персональную группу
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId.Value));

            _logger.LogInformation(
                "User {UserId} connected to VisualizationHub. ConnectionId: {ConnectionId}",
                userId.Value, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroup(userId.Value));

            _logger.LogInformation(
                "User {UserId} disconnected from VisualizationHub. ConnectionId: {ConnectionId}",
                userId.Value, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Подписка на обновления конкретного задания
    /// </summary>
    public async Task SubscribeToJob(Guid jobId)
    {
        var groupName = GetJobGroup(jobId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Connection {ConnectionId} subscribed to job {JobId}",
            Context.ConnectionId, jobId);
    }

    /// <summary>
    /// Отписка от обновлений задания
    /// </summary>
    public async Task UnsubscribeFromJob(Guid jobId)
    {
        var groupName = GetJobGroup(jobId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Connection {ConnectionId} unsubscribed from job {JobId}",
            Context.ConnectionId, jobId);
    }

    /// <summary>
    /// Подписка на обновления книги (все задания книги)
    /// </summary>
    public async Task SubscribeToBook(Guid bookId)
    {
        var groupName = GetBookGroup(bookId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Connection {ConnectionId} subscribed to book {BookId}",
            Context.ConnectionId, bookId);
    }

    /// <summary>
    /// Отписка от обновлений книги
    /// </summary>
    public async Task UnsubscribeFromBook(Guid bookId)
    {
        var groupName = GetBookGroup(bookId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static string GetUserGroup(Guid userId) => $"user_{userId}";
    private static string GetJobGroup(Guid jobId) => $"job_{jobId}";
    private static string GetBookGroup(Guid bookId) => $"book_{bookId}";
}

/// <summary>
/// Интерфейс клиентских методов SignalR
/// </summary>
public interface IVisualizationHubClient
{
    /// <summary>
    /// Уведомление о прогрессе задания
    /// </summary>
    Task OnJobProgress(JobProgressDto progress);

    /// <summary>
    /// Уведомление о завершении задания
    /// </summary>
    Task OnJobCompleted(VisualizationJobDto job);

    /// <summary>
    /// Уведомление об ошибке задания
    /// </summary>
    Task OnJobFailed(Guid jobId, string errorMessage);

    /// <summary>
    /// Уведомление об обновлении очереди
    /// </summary>
    Task OnQueueUpdate(QueueStatusDto queueStatus);

    /// <summary>
    /// Уведомление о новом изображении
    /// </summary>
    Task OnNewImage(GeneratedImageDto image);
}