using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Queries.GetVisualizationJob;

/// <summary>
/// Запрос получения задания визуализации по ID
/// </summary>
public sealed record GetVisualizationJobQuery : IRequest<Result<VisualizationJobDto>>
{
    public Guid JobId { get; init; }
    public bool IncludeImages { get; init; } = true;
}

/// <summary>
/// Handler для получения задания
/// </summary>
public sealed class GetVisualizationJobQueryHandler
    : IRequestHandler<GetVisualizationJobQuery, Result<VisualizationJobDto>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IVisualizationCacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVisualizationJobQueryHandler> _logger;

    public GetVisualizationJobQueryHandler(
        IVisualizationJobRepository jobRepository,
        IVisualizationCacheService cacheService,
        IMapper mapper,
        ILogger<GetVisualizationJobQueryHandler> logger)
    {
        _jobRepository = jobRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VisualizationJobDto>> Handle(
        GetVisualizationJobQuery request,
        CancellationToken cancellationToken)
    {
        var jobId = VisualizationJobId.From(request.JobId);

        // Пробуем из кэша
        var cached = await _cacheService.GetJobAsync(jobId, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Returning cached job {JobId}", request.JobId);
            return Result<VisualizationJobDto>.Success(cached);
        }

        // Из БД
        var job = request.IncludeImages
            ? await _jobRepository.GetByIdWithImagesAsync(jobId, cancellationToken)
            : await _jobRepository.GetByIdAsync(jobId, cancellationToken);

        if (job == null)
        {
            return Result<VisualizationJobDto>.Failure(
                Error.NotFound($"Visualization job {request.JobId} not found"));
        }

        var dto = _mapper.Map<VisualizationJobDto>(job);

        // Кэшируем если завершено
        if (job.Status.IsFinal)
        {
            await _cacheService.SetJobAsync(dto, TimeSpan.FromHours(24), cancellationToken);
        }

        return Result<VisualizationJobDto>.Success(dto);
    }
}
