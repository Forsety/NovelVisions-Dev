// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/Queries/GetUserJobs/GetUserJobsQuery.cs

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Repositories;

namespace NovelVision.Services.Visualization.Application.Queries.GetUserJobs;

/// <summary>
/// Запрос получения заданий пользователя
/// </summary>
public sealed record GetUserJobsQuery : IRequest<Result<UserJobsResultDto>>
{
    public Guid UserId { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; } = 20;
}

/// <summary>
/// Результат запроса заданий пользователя
/// </summary>
public sealed record UserJobsResultDto
{
    public IReadOnlyList<VisualizationJobSummaryDto> Jobs { get; init; } = Array.Empty<VisualizationJobSummaryDto>();
    public int TotalCount { get; init; }
}

/// <summary>
/// Handler для получения заданий пользователя
/// </summary>
public sealed class GetUserJobsQueryHandler : IRequestHandler<GetUserJobsQuery, Result<UserJobsResultDto>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserJobsQueryHandler> _logger;

    public GetUserJobsQueryHandler(
        IVisualizationJobRepository jobRepository,
        IMapper mapper,
        ILogger<GetUserJobsQueryHandler> logger)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserJobsResultDto>> Handle(
        GetUserJobsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting jobs for user {UserId}", request.UserId);

        var jobs = await _jobRepository.GetByUserIdAsync(
            request.UserId,
            request.Skip,
            request.Take,
            cancellationToken);

        var summaries = _mapper.Map<List<VisualizationJobSummaryDto>>(jobs);

        // TODO: Get total count from repository
        var result = new UserJobsResultDto
        {
            Jobs = summaries,
            TotalCount = summaries.Count + request.Skip // Approximate
        };

        return Result<UserJobsResultDto>.Success(result);
    }
}