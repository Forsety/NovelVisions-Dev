using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Queries.GetQueueStatus;

/// <summary>
/// Запрос получения статуса очереди
/// </summary>
public sealed record GetQueueStatusQuery : IRequest<Result<QueueStatusDto>>
{
    public Guid? JobId { get; init; }
}

/// <summary>
/// Handler для получения статуса очереди
/// </summary>
public sealed class GetQueueStatusQueryHandler
    : IRequestHandler<GetQueueStatusQuery, Result<QueueStatusDto>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IJobQueueService _queueService;
    private readonly IVisualizationCacheService _cacheService;

    public GetQueueStatusQueryHandler(
        IVisualizationJobRepository jobRepository,
        IJobQueueService queueService,
        IVisualizationCacheService cacheService)
    {
        _jobRepository = jobRepository;
        _queueService = queueService;
        _cacheService = cacheService;
    }

    public async Task<Result<QueueStatusDto>> Handle(
        GetQueueStatusQuery request,
        CancellationToken cancellationToken)
    {
        // Пробуем из кэша (только общий статус)
        if (!request.JobId.HasValue)
        {
            var cached = await _cacheService.GetQueueStatusAsync(cancellationToken);
            if (cached != null)
            {
                return Result<QueueStatusDto>.Success(cached);
            }
        }

        // Получаем статистику
        var queueLength = await _queueService.GetQueueLengthAsync(cancellationToken);
        var processingJobs = await _jobRepository.GetByStatusAsync(
            VisualizationJobStatus.Processing, 100, cancellationToken);
        var pendingJobs = await _jobRepository.GetByStatusAsync(
            VisualizationJobStatus.Pending, 100, cancellationToken);

        var position = 0;
        var estimatedWait = TimeSpan.Zero;

        if (request.JobId.HasValue)
        {
            var jobId = VisualizationJobId.From(request.JobId.Value);
            position = await _queueService.GetQueuePositionAsync(jobId, cancellationToken);
            estimatedWait = await _queueService.EstimateWaitTimeAsync(position, cancellationToken);
        }

        // Средн время обработки (примерное)
        var avgTime = processingJobs.Count > 0
            ? 30.0 // Default average
            : 30.0;

        var status = new QueueStatusDto
        {
            TotalInQueue = queueLength,
            Position = position,
            EstimatedWaitTime = estimatedWait,
            ProcessingCount = processingJobs.Count,
            PendingCount = pendingJobs.Count,
            AverageProcessingTimeSeconds = avgTime
        };

        // Кэшируем общий статус
        if (!request.JobId.HasValue)
        {
            await _cacheService.SetQueueStatusAsync(status, cancellationToken);
        }

        return Result<QueueStatusDto>.Success(status);
    }
}
