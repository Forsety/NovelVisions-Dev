using AutoMapper;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Repositories;

namespace NovelVision.Services.Visualization.Application.Queries.GetGeneratedImages;

/// <summary>
/// Запрос получения изображений по книге
/// </summary>
public sealed record GetGeneratedImagesByBookQuery : IRequest<Result<IReadOnlyList<GeneratedImageDto>>>
{
    public Guid BookId { get; init; }
    public int Skip { get; init; } = 0;
    public int Take { get; init; } = 50;
}

/// <summary>
/// Запрос получения изображений по странице
/// </summary>
public sealed record GetGeneratedImagesByPageQuery : IRequest<Result<IReadOnlyList<GeneratedImageDto>>>
{
    public Guid PageId { get; init; }
}

/// <summary>
/// Запрос получения выбранного изображения для страницы
/// </summary>
public sealed record GetSelectedImageForPageQuery : IRequest<Result<GeneratedImageDto?>>
{
    public Guid PageId { get; init; }
}

/// <summary>
/// Handler для получения изображений по книге
/// </summary>
public sealed class GetGeneratedImagesByBookQueryHandler
    : IRequestHandler<GetGeneratedImagesByBookQuery, Result<IReadOnlyList<GeneratedImageDto>>>
{
    private readonly IGeneratedImageRepository _imageRepository;
    private readonly IMapper _mapper;

    public GetGeneratedImagesByBookQueryHandler(
        IGeneratedImageRepository imageRepository,
        IMapper mapper)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<GeneratedImageDto>>> Handle(
        GetGeneratedImagesByBookQuery request,
        CancellationToken cancellationToken)
    {
        var images = await _imageRepository.GetByBookIdAsync(
            request.BookId,
            request.Skip,
            request.Take,
            cancellationToken);

        var dtos = _mapper.Map<List<GeneratedImageDto>>(images);
        return Result<IReadOnlyList<GeneratedImageDto>>.Success(dtos);
    }
}

/// <summary>
/// Handler для получения изображений по странице
/// </summary>
public sealed class GetGeneratedImagesByPageQueryHandler
    : IRequestHandler<GetGeneratedImagesByPageQuery, Result<IReadOnlyList<GeneratedImageDto>>>
{
    private readonly IGeneratedImageRepository _imageRepository;
    private readonly IMapper _mapper;

    public GetGeneratedImagesByPageQueryHandler(
        IGeneratedImageRepository imageRepository,
        IMapper mapper)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<GeneratedImageDto>>> Handle(
        GetGeneratedImagesByPageQuery request,
        CancellationToken cancellationToken)
    {
        var images = await _imageRepository.GetByPageIdAsync(request.PageId, cancellationToken);
        var dtos = _mapper.Map<List<GeneratedImageDto>>(images);
        return Result<IReadOnlyList<GeneratedImageDto>>.Success(dtos);
    }
}

/// <summary>
/// Handler для получения выбранного изображения
/// </summary>
public sealed class GetSelectedImageForPageQueryHandler
    : IRequestHandler<GetSelectedImageForPageQuery, Result<GeneratedImageDto?>>
{
    private readonly IGeneratedImageRepository _imageRepository;
    private readonly IMapper _mapper;

    public GetSelectedImageForPageQueryHandler(
        IGeneratedImageRepository imageRepository,
        IMapper mapper)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
    }

    public async Task<Result<GeneratedImageDto?>> Handle(
        GetSelectedImageForPageQuery request,
        CancellationToken cancellationToken)
    {
        var image = await _imageRepository.GetSelectedForPageAsync(request.PageId, cancellationToken);
        
        if (image == null)
        {
            return Result<GeneratedImageDto?>.Success(null);
        }

        var dto = _mapper.Map<GeneratedImageDto>(image);
        return Result<GeneratedImageDto?>.Success(dto);
    }
}
