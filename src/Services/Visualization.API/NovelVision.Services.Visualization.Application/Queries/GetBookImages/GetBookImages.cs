// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/Queries/GetBookImages/GetBookImagesQuery.cs

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Domain.Repositories;

namespace NovelVision.Services.Visualization.Application.Queries.GetBookImages;

/// <summary>
/// Запрос получения изображений книги
/// </summary>
public sealed record GetBookImagesQuery : IRequest<Result<BookImagesResultDto>>
{
    public Guid BookId { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; } = 50;
}

/// <summary>
/// Результат запроса изображений книги
/// </summary>
public sealed record BookImagesResultDto
{
    public IReadOnlyList<GeneratedImageDto> Images { get; init; } = Array.Empty<GeneratedImageDto>();
    public int TotalCount { get; init; }
}

/// <summary>
/// Handler для получения изображений книги
/// </summary>
public sealed class GetBookImagesQueryHandler : IRequestHandler<GetBookImagesQuery, Result<BookImagesResultDto>>
{
    private readonly IGeneratedImageRepository _imageRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBookImagesQueryHandler> _logger;

    public GetBookImagesQueryHandler(
        IGeneratedImageRepository imageRepository,
        IMapper mapper,
        ILogger<GetBookImagesQueryHandler> logger)
    {
        _imageRepository = imageRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BookImagesResultDto>> Handle(
        GetBookImagesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting images for book {BookId}", request.BookId);

        var images = await _imageRepository.GetByBookIdAsync(
            request.BookId,
            request.Skip,
            request.Take,
            cancellationToken);

        var totalCount = await _imageRepository.GetCountByBookIdAsync(
            request.BookId,
            cancellationToken);

        var imageDtos = _mapper.Map<List<GeneratedImageDto>>(images);

        var result = new BookImagesResultDto
        {
            Images = imageDtos,
            TotalCount = totalCount
        };

        return Result<BookImagesResultDto>.Success(result);
    }
}