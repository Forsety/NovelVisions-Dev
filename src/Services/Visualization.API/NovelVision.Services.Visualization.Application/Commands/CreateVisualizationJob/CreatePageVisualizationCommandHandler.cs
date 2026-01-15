using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Application.Commands.CreateVisualizationJob;

/// <summary>
/// Handler для создания визуализации страницы
/// </summary>
public sealed class CreatePageVisualizationCommandHandler
    : IRequestHandler<CreatePageVisualizationCommand, Result<VisualizationJobDto>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly ICatalogService _catalogService;
    private readonly IJobQueueService _queueService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePageVisualizationCommandHandler> _logger;

    public CreatePageVisualizationCommandHandler(
        IVisualizationJobRepository jobRepository,
        ICatalogService catalogService,
        IJobQueueService queueService,
        IMapper mapper,
        ILogger<CreatePageVisualizationCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _catalogService = catalogService;
        _queueService = queueService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VisualizationJobDto>> Handle(
        CreatePageVisualizationCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating page visualization for BookId: {BookId}, PageId: {PageId}, UserId: {UserId}",
            request.BookId, request.PageId, request.UserId);

        // Проверяем, поддерживает ли книга визуализацию
        var isEnabledResult = await _catalogService.IsVisualizationEnabledAsync(
            request.BookId, cancellationToken);

        if (isEnabledResult.IsFailure)
        {
            return Result<VisualizationJobDto>.Failure(isEnabledResult.Error);
        }

        if (!isEnabledResult.Value)
        {
            return Result<VisualizationJobDto>.Failure(
                Error.Validation("Visualization is not enabled for this book"));
        }

        // Определяем провайдера
        var provider = !string.IsNullOrEmpty(request.PreferredProvider)
            ? AIModelProvider.FromApiName(request.PreferredProvider)
            : AIModelProvider.DallE3;

        // Маппим параметры
        var parameters = request.Parameters != null
            ? _mapper.Map<GenerationParameters>(request.Parameters)
            : GenerationParameters.Default();

        // Создаём задание
        var jobResult = VisualizationJob.CreateForButton(
            request.BookId,
            request.PageId,
            request.UserId,
            provider,
            parameters);

        if (jobResult.IsFailure)
        {
            return Result<VisualizationJobDto>.Failure(jobResult.Error);
        }

        var job = jobResult.Value;

        // Добавляем в очередь
        var queuePosition = await _queueService.EnqueueJobAsync(
            job.Id, job.Priority, cancellationToken);

        var estimatedWait = await _queueService.EstimateWaitTimeAsync(
            queuePosition, cancellationToken);

        // Ставим в очередь
        var enqueueResult = job.Enqueue(queuePosition, estimatedWait);
        if (enqueueResult.IsFailure)
        {
            return Result<VisualizationJobDto>.Failure(enqueueResult.Error);
        }

        // Сохраняем
        await _jobRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created visualization job {JobId} at queue position {Position}",
            job.Id.Value, queuePosition);

        var dto = _mapper.Map<VisualizationJobDto>(job);
        return Result<VisualizationJobDto>.Success(dto);
    }
}
