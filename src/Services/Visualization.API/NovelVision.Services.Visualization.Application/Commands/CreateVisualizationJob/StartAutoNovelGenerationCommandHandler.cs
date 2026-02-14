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
/// Handler для запуска авто веб-новеллы
/// </summary>
public sealed class StartAutoNovelGenerationCommandHandler
    : IRequestHandler<StartAutoNovelGenerationCommand, Result<IReadOnlyList<VisualizationJobSummaryDto>>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly ICatalogService _catalogService;
    private readonly IJobQueueService _queueService;
    private readonly IMapper _mapper;
    private readonly ILogger<StartAutoNovelGenerationCommandHandler> _logger;

    public StartAutoNovelGenerationCommandHandler(
        IVisualizationJobRepository jobRepository,
        ICatalogService catalogService,
        IJobQueueService queueService,
        IMapper mapper,
        ILogger<StartAutoNovelGenerationCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _catalogService = catalogService;
        _queueService = queueService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<VisualizationJobSummaryDto>>> Handle(
        StartAutoNovelGenerationCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting auto novel generation for BookId: {BookId}",
            request.BookId);

        // Проверяем книгу
        var bookInfoResult = await _catalogService.GetBookInfoAsync(
            request.BookId, cancellationToken);

        if (bookInfoResult.IsFailure)
        {
            return Result<IReadOnlyList<VisualizationJobSummaryDto>>.Failure(bookInfoResult.Error);
        }

        var bookInfo = bookInfoResult.Value;

        if (!bookInfo.VisualizationEnabled)
        {
            return Result<IReadOnlyList<VisualizationJobSummaryDto>>.Failure(
                Error.Validation("Visualization is not enabled for this book"));
        }

        // Получаем все страницы
        var pagesResult = await _catalogService.GetBookPagesAsync(
            request.BookId, cancellationToken);

        if (pagesResult.IsFailure)
        {
            return Result<IReadOnlyList<VisualizationJobSummaryDto>>.Failure(pagesResult.Error);
        }

        var pages = pagesResult.Value;

        // Фильтруем страницы если нужно пропустить существующие
        if (request.SkipExistingVisualizations)
        {
            pages = pages.Where(p => !p.HasVisualization).ToList();
        }

        if (!pages.Any())
        {
            _logger.LogInformation("No pages to visualize for BookId: {BookId}", request.BookId);
            return Result<IReadOnlyList<VisualizationJobSummaryDto>>.Success(
                Array.Empty<VisualizationJobSummaryDto>());
        }

        // Определяем провайдера
        var provider = !string.IsNullOrEmpty(request.PreferredProvider)
            ? AIModelProvider.FromApiName(request.PreferredProvider)
            : AIModelProvider.DallE3;

        var parameters = GenerationParameters.Default();
        var createdJobs = new List<VisualizationJob>();

        // Создаём задания для каждой страницы
        foreach (var page in pages)
        {
            var jobResult = VisualizationJob.CreateForAutoNovel(
                request.BookId,
                page.Id,
                page.ChapterId,
                request.UserId,
                provider,
                parameters);

            if (jobResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to create job for page {PageId}: {Error}",
                    page.Id, jobResult.Error.Message);
                continue;
            }

            var job = jobResult.Value;

            // Добавляем в очередь с низким приоритетом (batch)
            var queuePosition = await _queueService.EnqueueJobAsync(
                job.Id, job.Priority, cancellationToken);

            var estimatedWait = await _queueService.EstimateWaitTimeAsync(
                queuePosition, cancellationToken);

            job.Enqueue(queuePosition, estimatedWait);
            
            createdJobs.Add(job);
        }

        await _jobRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created {Count} auto novel visualization jobs for BookId: {BookId}",
            createdJobs.Count, request.BookId);

        var dtos = _mapper.Map<List<VisualizationJobSummaryDto>>(createdJobs);
        return Result<IReadOnlyList<VisualizationJobSummaryDto>>.Success(dtos);
    }
}
