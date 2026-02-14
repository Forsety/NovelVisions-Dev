using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Commands.SelectImage;

/// <summary>
/// Команда выбора изображения как основного
/// </summary>
public sealed record SelectImageCommand : IRequest<Result<bool>>
{
    public Guid JobId { get; init; }
    public Guid ImageId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Handler для выбора изображения
/// </summary>
public sealed class SelectImageCommandHandler : IRequestHandler<SelectImageCommand, Result<bool>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly ICatalogService _catalogService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly ILogger<SelectImageCommandHandler> _logger;

    public SelectImageCommandHandler(
        IVisualizationJobRepository jobRepository,
        ICatalogService catalogService,
        IVisualizationCacheService cacheService,
        ILogger<SelectImageCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _catalogService = catalogService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        SelectImageCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Selecting image {ImageId} for job {JobId}",
            request.ImageId, request.JobId);

        var jobId = VisualizationJobId.From(request.JobId);
        var imageId = GeneratedImageId.From(request.ImageId);

        var job = await _jobRepository.GetByIdWithImagesAsync(jobId, cancellationToken);
        if (job == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Job {request.JobId} not found"));
        }

        // Проверяем права
        if (job.UserId != request.UserId)
        {
            return Result<bool>.Failure(
                Error.Forbidden("You can only modify your own visualization jobs"));
        }

        // Выбираем изображение
        var selectResult = job.SelectImage(imageId);
        if (selectResult.IsFailure)
        {
            return selectResult;
        }

        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Обновляем страницу в Catalog.API
        if (job.PageId.HasValue && job.SelectedImage != null)
        {
            await _catalogService.UpdatePageVisualizationStatusAsync(
                job.PageId.Value,
                true,
                job.SelectedImage.ImageUrl,
                cancellationToken);
        }

        // Инвалидируем кэш
        await _cacheService.InvalidateJobAsync(jobId, cancellationToken);

        _logger.LogInformation(
            "Image {ImageId} selected for job {JobId}",
            request.ImageId, request.JobId);

        return Result<bool>.Success(true);
    }
}
