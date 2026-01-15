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
/// Handler для создания визуализации выделенного текста
/// </summary>
public sealed class CreateTextSelectionVisualizationCommandHandler
    : IRequestHandler<CreateTextSelectionVisualizationCommand, Result<VisualizationJobDto>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly ICatalogService _catalogService;
    private readonly IJobQueueService _queueService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateTextSelectionVisualizationCommandHandler> _logger;

    public CreateTextSelectionVisualizationCommandHandler(
        IVisualizationJobRepository jobRepository,
        ICatalogService catalogService,
        IJobQueueService queueService,
        IMapper mapper,
        ILogger<CreateTextSelectionVisualizationCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _catalogService = catalogService;
        _queueService = queueService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VisualizationJobDto>> Handle(
        CreateTextSelectionVisualizationCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating text selection visualization for BookId: {BookId}, PageId: {PageId}",
            request.BookId, request.PageId);

        // Проверяем книгу
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

        // Создаём TextSelection
        var textSelection = TextSelection.Create(
            request.SelectedText,
            request.StartPosition,
            request.EndPosition,
            request.PageId,
            request.ChapterId,
            request.ContextBefore,
            request.ContextAfter);

        // Определяем провайдера
        var provider = !string.IsNullOrEmpty(request.PreferredProvider)
            ? AIModelProvider.FromApiName(request.PreferredProvider)
            : AIModelProvider.DallE3;

        // Маппим параметры
        var parameters = request.Parameters != null
            ? _mapper.Map<GenerationParameters>(request.Parameters)
            : GenerationParameters.Default();

        // Создаём задание
        var jobResult = VisualizationJob.CreateForTextSelection(
            request.BookId,
            textSelection,
            request.UserId,
            provider,
            parameters);

        if (jobResult.IsFailure)
        {
            return Result<VisualizationJobDto>.Failure(jobResult.Error);
        }

        var job = jobResult.Value;

        // Добавляем в очередь (высокий приоритет для user-initiated)
        var queuePosition = await _queueService.EnqueueJobAsync(
            job.Id, job.Priority, cancellationToken);

        var estimatedWait = await _queueService.EstimateWaitTimeAsync(
            queuePosition, cancellationToken);

        var enqueueResult = job.Enqueue(queuePosition, estimatedWait);
        if (enqueueResult.IsFailure)
        {
            return Result<VisualizationJobDto>.Failure(enqueueResult.Error);
        }

        // Сохраняем
        await _jobRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created text selection visualization job {JobId} for text length {TextLength}",
            job.Id.Value, request.SelectedText.Length);

        var dto = _mapper.Map<VisualizationJobDto>(job);
        return Result<VisualizationJobDto>.Success(dto);
    }
}
