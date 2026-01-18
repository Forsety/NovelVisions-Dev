// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/Queries/GetJob/GetJobQuery.cs

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Queries.GetJob;

/// <summary>
/// Запрос получения задания по ID
/// </summary>
public sealed record GetJobQuery : IRequest<Result<VisualizationJobDto>>
{
    public Guid JobId { get; init; }
}

/// <summary>
/// Handler для получения задания
/// </summary>
public sealed class GetJobQueryHandler : IRequestHandler<GetJobQuery, Result<VisualizationJobDto>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IVisualizationCacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetJobQueryHandler> _logger;

    public GetJobQueryHandler(
        IVisualizationJobRepository jobRepository,
        IVisualizationCacheService cacheService,
        IMapper mapper,
        ILogger<GetJobQueryHandler> logger)
    {
        _jobRepository = jobRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VisualizationJobDto>> Handle(
        GetJobQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting job {JobId}", request.JobId);

        var jobId = VisualizationJobId.From(request.JobId);

        // Try cache first
        var cached = await _cacheService.GetJobAsync(jobId, cancellationToken);
        if (cached != null)
        {
            return Result<VisualizationJobDto>.Success(cached);
        }

        // Get from DB
        var job = await _jobRepository.GetByIdWithImagesAsync(jobId, cancellationToken);

        if (job == null)
        {
            return Result<VisualizationJobDto>.Failure(
                Error.NotFound($"Job {request.JobId} not found"));
        }

        var dto = _mapper.Map<VisualizationJobDto>(job);

        // Cache the result
        await _cacheService.SetJobAsync(dto, cancellationToken: cancellationToken);

        return Result<VisualizationJobDto>.Success(dto);
    }
}