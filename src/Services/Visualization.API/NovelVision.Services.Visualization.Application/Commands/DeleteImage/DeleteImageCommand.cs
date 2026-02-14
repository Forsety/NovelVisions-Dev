using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;

namespace NovelVision.Services.Visualization.Application.Commands.DeleteImage;

/// <summary>
/// Команда удаления изображения
/// </summary>
public sealed record DeleteImageCommand : IRequest<Result<bool>>
{
    public Guid JobId { get; init; }
    public Guid ImageId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Handler для удаления изображения
/// </summary>
public sealed class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Result<bool>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly IImageStorageService _storageService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly ILogger<DeleteImageCommandHandler> _logger;

    public DeleteImageCommandHandler(
        IVisualizationJobRepository jobRepository,
        IImageStorageService storageService,
        IVisualizationCacheService cacheService,
        ILogger<DeleteImageCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _storageService = storageService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteImageCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting image {ImageId} from job {JobId}",
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

        // Находим изображение для получения blob path
        var image = job.Images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Image {request.ImageId} not found"));
        }

        // Удаляем из агрегата (soft delete)
        var deleteResult = job.DeleteImage(imageId);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        await _jobRepository.SaveChangesAsync(cancellationToken);

        // Удаляем из хранилища (можно делать асинхронно через очередь)
        if (!string.IsNullOrEmpty(image.Metadata.BlobPath))
        {
            await _storageService.DeleteImageAsync(image.Metadata.BlobPath, cancellationToken);
        }

        // Инвалидируем кэш
        await _cacheService.InvalidateJobAsync(jobId, cancellationToken);

        _logger.LogInformation(
            "Image {ImageId} deleted from job {JobId}",
            request.ImageId, request.JobId);

        return Result<bool>.Success(true);
    }
}
