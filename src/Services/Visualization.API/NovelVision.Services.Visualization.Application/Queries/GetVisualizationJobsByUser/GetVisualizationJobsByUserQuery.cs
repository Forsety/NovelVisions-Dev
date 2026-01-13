using AutoMapper;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Repositories;

namespace NovelVision.Services.Visualization.Application.Queries.GetVisualizationJobsByUser;

/// <summary>
/// Запрос получения заданий пользователя
/// </summary>
public sealed record GetVisualizationJobsByUserQuery : IRequest<Result<PagedResult<VisualizationJobSummaryDto>>>
{
    public Guid UserId { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 20;
}

/// <summary>
/// Результат с пагинацией
/// </summary>
public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
    public bool HasMore => Skip + Items.Count < TotalCount;
}

/// <summary>
/// Handler для получения заданий пользователя
/// </summary>
public sealed class GetVisualizationJobsByUserQueryHandler
    : IRequestHandler<GetVisualizationJobsByUserQuery, Result<PagedResult<VisualizationJobSummaryDto>>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public GetVisualizationJobsByUserQueryHandler(
        IVisualizationJobRepository jobRepository,
        IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<VisualizationJobSummaryDto>>> Handle(
        GetVisualizationJobsByUserQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetByUserIdAsync(
            request.UserId,
            request.Skip,
            request.Take + 1, // +1 to check if there are more
            cancellationToken);

        var hasMore = jobs.Count > request.Take;
        var items = jobs.Take(request.Take).ToList();

        var dtos = _mapper.Map<List<VisualizationJobSummaryDto>>(items);

        var result = new PagedResult<VisualizationJobSummaryDto>
        {
            Items = dtos,
            Skip = request.Skip,
            Take = request.Take,
            TotalCount = request.Skip + items.Count + (hasMore ? 1 : 0) // Approximation
        };

        return Result<PagedResult<VisualizationJobSummaryDto>>.Success(result);
    }
}
