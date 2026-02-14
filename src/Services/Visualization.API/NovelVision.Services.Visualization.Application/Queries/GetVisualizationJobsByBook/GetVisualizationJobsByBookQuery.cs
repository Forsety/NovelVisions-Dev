using AutoMapper;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Repositories;

namespace NovelVision.Services.Visualization.Application.Queries.GetVisualizationJobsByBook;

/// <summary>
/// Запрос получения заданий по книге
/// </summary>
public sealed record GetVisualizationJobsByBookQuery : IRequest<Result<IReadOnlyList<VisualizationJobSummaryDto>>>
{
    public Guid BookId { get; init; }
}

/// <summary>
/// Запрос получения заданий по странице
/// </summary>
public sealed record GetVisualizationJobsByPageQuery : IRequest<Result<IReadOnlyList<VisualizationJobSummaryDto>>>
{
    public Guid PageId { get; init; }
}

/// <summary>
/// Handler для получения заданий по книге
/// </summary>
public sealed class GetVisualizationJobsByBookQueryHandler
    : IRequestHandler<GetVisualizationJobsByBookQuery, Result<IReadOnlyList<VisualizationJobSummaryDto>>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public GetVisualizationJobsByBookQueryHandler(
        IVisualizationJobRepository jobRepository,
        IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<VisualizationJobSummaryDto>>> Handle(
        GetVisualizationJobsByBookQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetByBookIdAsync(request.BookId, cancellationToken);
        var dtos = _mapper.Map<List<VisualizationJobSummaryDto>>(jobs);
        return Result<IReadOnlyList<VisualizationJobSummaryDto>>.Success(dtos);
    }
}

/// <summary>
/// Handler для получения заданий по странице
/// </summary>
public sealed class GetVisualizationJobsByPageQueryHandler
    : IRequestHandler<GetVisualizationJobsByPageQuery, Result<IReadOnlyList<VisualizationJobSummaryDto>>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public GetVisualizationJobsByPageQueryHandler(
        IVisualizationJobRepository jobRepository,
        IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<VisualizationJobSummaryDto>>> Handle(
        GetVisualizationJobsByPageQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetByPageIdAsync(request.PageId, cancellationToken);
        var dtos = _mapper.Map<List<VisualizationJobSummaryDto>>(jobs);
        return Result<IReadOnlyList<VisualizationJobSummaryDto>>.Success(dtos);
    }
}
