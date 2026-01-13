using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Commands.CancelVisualizationJob;

/// <summary>
/// Команда отмены задания визуализации
/// </summary>
public sealed record CancelVisualizationJobCommand : IRequest<Result<bool>>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
    public string Reason { get; init; } = "Cancelled by user";
}

/// <summary>
/// Handler для отмены задания
/// </summary>
public sealed class CancelVisualizationJobCommandHandler
    : IRequestHandler<CancelVisualizationJobCommand, Result<bool>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IJobQueueService _queueService;
    private readonly IVisualizationNotificationService _notificationService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly ILogger<CancelVisualizationJobCommandHandler> _logger;

    public CancelVisualizationJobCommandHandler(
        IVisualizationJobRepository jobRepository,
        IJobQueueService queueService,
        IVisualizationNotificationService notificationService,
        IVisualizationCacheService cacheService,
        ILogger<CancelVisualizationJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _queueService = queueService;
        _notificationService = notificationService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CancelVisualizationJobCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Cancelling visualization job {JobId} by user {UserId}",
            request.JobId, request.UserId);

        var jobId = VisualizationJobId.From(request.JobId);
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);

        if (job == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Job {request.JobId} not found"));
        }

        // Проверяем права (пользователь может отменить только своё задание)
        if (job.UserId != request.UserId)
        {
            return Result<bool>.Failure(
                Error.Forbidden("You can only cancel your own visualization jobs"));
        }

        // Отменяем
        var cancelResult = job.Cancel(request.Reason, request.UserId);
        if (cancelResult.IsFailure)
        {
            return Result<bool>.Failure(cancelResult.Error);
        }

        // Удаляем из очереди
        await _queueService.RemoveFromQueueAsync(jobId, cancellationToken);

        // Сохраняем
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Инвалидируем кэш
        await _cacheService.InvalidateJobAsync(jobId, cancellationToken);

        // Уведомляем
        await _notificationService.NotifyJobFailedAsync(
            job.UserId,
            job.Id.Value,
            $"Job cancelled: {request.Reason}",
            cancellationToken);

        _logger.LogInformation("Visualization job {JobId} cancelled", request.JobId);

        return Result<bool>.Success(true);
    }
}
