// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/Commands/CancelJob/CancelJobCommand.cs

using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Commands.CancelJob;

/// <summary>
/// Команда отмены задания
/// </summary>
public sealed record CancelJobCommand : IRequest<Result<bool>>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Handler для отмены задания
/// </summary>
public sealed class CancelJobCommandHandler : IRequestHandler<CancelJobCommand, Result<bool>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IJobQueueService _queueService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly ILogger<CancelJobCommandHandler> _logger;

    public CancelJobCommandHandler(
        IVisualizationJobRepository jobRepository,
        IJobQueueService queueService,
        IVisualizationCacheService cacheService,
        ILogger<CancelJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _queueService = queueService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CancelJobCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling job {JobId}", request.JobId);

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
                Error.Forbidden("You can only cancel your own jobs"));
        }

        // Проверяем можно ли отменить
        if (!job.CanCancel)
        {
            return Result<bool>.Failure(
                Error.Validation($"Job cannot be cancelled in status {job.Status.Name}"));
        }

        // Отменяем
        var cancelResult = job.Cancel("Cancelled by user");
        if (cancelResult.IsFailure)
        {
            return Result<bool>.Failure(cancelResult.Error);
        }

        // Удаляем из очереди
        await _queueService.RemoveFromQueueAsync(jobId, cancellationToken);

        // Сохраняем
        _jobRepository.Update(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Инвалидируем кэш
        await _cacheService.InvalidateJobAsync(jobId, cancellationToken);

        _logger.LogInformation("Job {JobId} cancelled successfully", request.JobId);

        return Result<bool>.Success(true);
    }
}