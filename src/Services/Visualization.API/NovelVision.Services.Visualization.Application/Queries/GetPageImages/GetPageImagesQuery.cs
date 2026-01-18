// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/Queries/GetPageImages/GetPageImagesQuery.cs

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Repositories;

namespace NovelVision.Services.Visualization.Application.Queries.GetPageImages;

/// <summary>
/// Запрос получения изображений страницы
/// </summary>
public sealed record GetPageImagesQuery : IRequest<Result<IReadOnlyList<GeneratedImageDto>>>
{
    public Guid PageId { get; init; }
}

/// <summary>
/// Запрос получения выбранного изображения страницы
/// </summary>
public sealed record GetSelectedImageQuery : IRequest<Result<GeneratedImageDto>>
{
    public Guid PageId { get; init; }
}

/// <summary>
/// Handler для получения изображений страницы
/// </summary>
public sealed class GetPageImagesQueryHandler : IRequestHandler<GetPageImagesQuery, Result<IReadOnlyList<GeneratedImageDto>>>
{
    private readonly IGeneratedImageRepository _imageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPageImagesQueryHandler> _logger;

    public GetPageImagesQueryHandler(
        IGeneratedImageRepository imageRepository,
        IMapper mapper,
        ILogger<GetPageImagesQueryHandler> logger)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<GeneratedImageDto>>> Handle(
        GetPageImagesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting images for page {PageId}", request.PageId);

        var images = await _imageRepository.GetByPageIdAsync(
            request.PageId,
            cancellationToken);

        var imageDtos = _mapper.Map<List<GeneratedImageDto>>(images);

        return Result<IReadOnlyList<GeneratedImageDto>>.Success(imageDtos);
    }
}

/// <summary>
/// Handler для получения выбранного изображения
/// </summary>
public sealed class GetSelectedImageQueryHandler : IRequestHandler<GetSelectedImageQuery, Result<GeneratedImageDto>>
{
    private readonly IGeneratedImageRepository _imageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSelectedImageQueryHandler> _logger;

    public GetSelectedImageQueryHandler(
        IGeneratedImageRepository imageRepository,
        IMapper mapper,
        ILogger<GetSelectedImageQueryHandler> logger)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<GeneratedImageDto>> Handle(
        GetSelectedImageQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting selected image for page {PageId}", request.PageId);

        var image = await _imageRepository.GetSelectedForPageAsync(
            request.PageId,
            cancellationToken);

        if (image == null)
        {
            return Result<GeneratedImageDto>.Failure(
                Error.NotFound($"No selected image for page {request.PageId}"));
        }

        var dto = _mapper.Map<GeneratedImageDto>(image);

        return Result<GeneratedImageDto>.Success(dto);
    }
}