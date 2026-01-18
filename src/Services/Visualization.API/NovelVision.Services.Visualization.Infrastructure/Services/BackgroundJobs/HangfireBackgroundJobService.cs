// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/BackgroundJobs/HangfireBackgroundJobService.cs

using Hangfire;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Infrastructure.Services.BackgroundJobs;

/// <summary>
/// Hangfire реализация сервиса фоновых задач
/// </summary>
public sealed class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<HangfireBackgroundJobService> _logger;

    public HangfireBackgroundJobService(
        IBackgroundJobClient jobClient,
        ILogger<HangfireBackgroundJobService> logger)
    {
        _jobClient = jobClient;
        _logger = logger;
    }

    public Task EnqueueProcessJobAsync(
        VisualizationJobId jobId,
        CancellationToken cancellationToken = default)
    {
        var hangfireJobId = _jobClient.Enqueue<IVisualizationJobProcessor>(
            processor => processor.ProcessAsync(jobId.Value, CancellationToken.None));

        _logger.LogInformation(
            "Enqueued Hangfire job {HangfireJobId} for visualization job {JobId}",
            hangfireJobId, jobId.Value);

        return Task.CompletedTask;
    }

    public Task ScheduleProcessJobAsync(
        VisualizationJobId jobId,
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        var hangfireJobId = _jobClient.Schedule<IVisualizationJobProcessor>(
            processor => processor.ProcessAsync(jobId.Value, CancellationToken.None),
            delay);

        _logger.LogInformation(
            "Scheduled Hangfire job {HangfireJobId} for visualization job {JobId} with delay {Delay}",
            hangfireJobId, jobId.Value, delay);

        return Task.CompletedTask;
    }

    public Task CancelScheduledJobAsync(
        string scheduledJobId,
        CancellationToken cancellationToken = default)
    {
        var deleted = _jobClient.Delete(scheduledJobId);

        if (deleted)
        {
            _logger.LogInformation("Cancelled Hangfire job {HangfireJobId}", scheduledJobId);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Интерфейс для Hangfire job processor
/// </summary>
public interface IVisualizationJobProcessor
{
    Task ProcessAsync(Guid jobId, CancellationToken cancellationToken);
}