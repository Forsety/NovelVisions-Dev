// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/Commands/RetryJob/RetryJobCommand.cs

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Commands.RetryJob;

/// <summary>
/// Команда повторной попытки задания
/// </summary>
public sealed record RetryJobCommand : IRequest<Result<VisualizationJobDto>>
{
    public Guid JobId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Handler для повторной попытки
/// </summary>
public sealed class RetryJobCommandHandler : IRequestHandler<RetryJobCommand, Result<VisualizationJobDto>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IJobQueueService _queueService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<RetryJobCommandHandler> _logger;

    public RetryJobCommandHandler(
        IVisualizationJobRepository jobRepository,
        IJobQueueService queueService,
        IBackgroundJobService backgroundJobService,
        IVisualizationCacheService cacheService,
        IMapper mapper,
        ILogger<RetryJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _queueService = queueService;
        _backgroundJobService = backgroundJobService;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VisualizationJobDto>> Handle(
        RetryJobCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrying job {JobId}", request.JobId);

        var jobId = VisualizationJobId.From(request.JobId);
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);

        if (job == null)
        {
            return Result<VisualizationJobDto>.Failure(
                Error.NotFound($"Job {request.JobId} not found"));
        }

        // Проверяем права
        if (job.UserId != request.UserId)
        {
            return Result<VisualizationJobDto>.Failure(
                Error.Forbidden("You can only retry your own jobs"));
        }

        // Проверяем можно ли повторить
        if (!job.CanRetry)
        {
            return Result<VisualizationJobDto>.Failure(
                Error.Validation($"Job cannot be retried in status {job.Status.Name}"));
        }

        // Retry
        var retryResult = job.Retry();
        if (retryResult.IsFailure)
        {
            return Result<VisualizationJobDto>.Failure(retryResult.Error);
        }

        // Добавляем в очередь
        var queuePosition = await _queueService.EnqueueJobAsync(
            jobId, job.Priority, cancellationToken);

        // Сохраняем
        _jobRepository.Update(job);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Запускаем обработку
        await _backgroundJobService.EnqueueProcessJobAsync(jobId, cancellationToken);

        // Инвалидируем кэш
        await _cacheService.InvalidateJobAsync(jobId, cancellationToken);

        _logger.LogInformation("Job {JobId} retry initiated, queue position: {Position}",
            request.JobId, queuePosition);

        var dto = _mapper.Map<VisualizationJobDto>(job);
        return Result<VisualizationJobDto>.Success(dto);
    }
}