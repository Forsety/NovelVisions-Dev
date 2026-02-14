using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Commands.RetryVisualizationJob;

/// <summary>
/// Команда повторной попытки визуализации
/// </summary>
public sealed record RetryVisualizationJobCommand : IRequest<Result<bool>>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Handler для повторной попытки
/// </summary>
public sealed class RetryVisualizationJobCommandHandler
    : IRequestHandler<RetryVisualizationJobCommand, Result<bool>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IJobQueueService _queueService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly ILogger<RetryVisualizationJobCommandHandler> _logger;

    public RetryVisualizationJobCommandHandler(
        IVisualizationJobRepository jobRepository,
        IJobQueueService queueService,
        IVisualizationCacheService cacheService,
        ILogger<RetryVisualizationJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _queueService = queueService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        RetryVisualizationJobCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Retrying visualization job {JobId} by user {UserId}",
            request.JobId, request.UserId);

        var jobId = VisualizationJobId.From(request.JobId);
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);

        if (job == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Job {request.JobId} not found"));
        }

        // Проверяем права
        if (job.UserId != request.UserId)
        {
            return Result<bool>.Failure(
                Error.Forbidden("You can only retry your own visualization jobs"));
        }

        // Пытаемся повторить
        var retryResult = job.Retry();
        if (retryResult.IsFailure)
        {
            return Result<bool>.Failure(retryResult.Error);
        }

        // Добавляем обратно в очередь с повышенным приоритетом
        var queuePosition = await _queueService.EnqueueJobAsync(
            jobId, job.Priority + 5, cancellationToken); // Boost priority for retries

        var estimatedWait = await _queueService.EstimateWaitTimeAsync(
            queuePosition, cancellationToken);

        job.Enqueue(queuePosition, estimatedWait);

        // Сохраняем
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Инвалидируем кэш
        await _cacheService.InvalidateJobAsync(jobId, cancellationToken);

        _logger.LogInformation(
            "Visualization job {JobId} queued for retry (attempt {Attempt})",
            request.JobId, job.RetryCount);

        return Result<bool>.Success(true);
    }
}
