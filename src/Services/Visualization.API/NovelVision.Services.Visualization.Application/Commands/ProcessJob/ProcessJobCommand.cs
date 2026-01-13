using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.DTOs;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.Repositories;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Application.Commands.ProcessJob;

/// <summary>
/// Команда обработки задания (вызывается Background Worker)
/// </summary>
public sealed record ProcessJobCommand : IRequest<Result<bool>>
{
    public Guid JobId { get; init; }
}

/// <summary>
/// Handler для обработки задания
/// </summary>
public sealed class ProcessJobCommandHandler : IRequestHandler<ProcessJobCommand, Result<bool>>
{
    private readonly IVisualizationJobRepository _jobRepository;
    private readonly ICatalogService _catalogService;
    private readonly IPromptGenService _promptGenService;
    private readonly IAIImageGeneratorService _aiGeneratorService;
    private readonly IImageStorageService _storageService;
    private readonly IVisualizationNotificationService _notificationService;
    private readonly IVisualizationCacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessJobCommandHandler> _logger;

    public ProcessJobCommandHandler(
        IVisualizationJobRepository jobRepository,
        ICatalogService catalogService,
        IPromptGenService promptGenService,
        IAIImageGeneratorService aiGeneratorService,
        IImageStorageService storageService,
        IVisualizationNotificationService notificationService,
        IVisualizationCacheService cacheService,
        IMapper mapper,
        ILogger<ProcessJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _catalogService = catalogService;
        _promptGenService = promptGenService;
        _aiGeneratorService = aiGeneratorService;
        _storageService = storageService;
        _notificationService = notificationService;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ProcessJobCommand request,
        CancellationToken cancellationToken)
    {
        var jobId = VisualizationJobId.From(request.JobId);
        
        _logger.LogInformation("Processing visualization job {JobId}", request.JobId);

        var job = await _jobRepository.GetByIdWithImagesAsync(jobId, cancellationToken);
        if (job == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Job {request.JobId} not found"));
        }

        try
        {
            // Step 1: Получаем текст для визуализации
            var originalText = await GetOriginalTextAsync(job, cancellationToken);
            if (string.IsNullOrEmpty(originalText))
            {
                return await FailJobAsync(job, "Could not retrieve text for visualization", cancellationToken);
            }

            // Step 2: Генерируем промпт через PromptGen.API
            await NotifyProgressAsync(job, VisualizationJobStatus.GeneratingPrompt, 10, cancellationToken);
            
            var startPromptResult = job.StartPromptGeneration(originalText);
            if (startPromptResult.IsFailure)
            {
                return await FailJobAsync(job, startPromptResult.Error.Message, cancellationToken);
            }
            await _jobRepository.SaveChangesAsync(cancellationToken);

            var promptResult = await _promptGenService.GeneratePromptAsync(
                originalText,
                job.PreferredProvider,
                job.BookId,
                cancellationToken: cancellationToken);

            if (promptResult.IsFailure)
            {
                return await FailJobAsync(job, $"Prompt generation failed: {promptResult.Error.Message}", cancellationToken);
            }

            var promptData = PromptData.FromPromptGenResponse(
                originalText,
                promptResult.Value.EnhancedPrompt,
                promptResult.Value.TargetModel,
                promptResult.Value.NegativePrompt,
                promptResult.Value.Style,
                promptResult.Value.Parameters);

            var setPromptResult = job.SetPromptData(promptData);
            if (setPromptResult.IsFailure)
            {
                return await FailJobAsync(job, setPromptResult.Error.Message, cancellationToken);
            }
            await _jobRepository.SaveChangesAsync(cancellationToken);

            // Step 3: Отправляем на AI генерацию
            await NotifyProgressAsync(job, VisualizationJobStatus.Processing, 30, cancellationToken);

            var generationResult = await _aiGeneratorService.StartGenerationAsync(
                promptData.EnhancedPrompt,
                promptData.NegativePrompt,
                job.PreferredProvider,
                job.Parameters,
                cancellationToken);

            if (generationResult.IsFailure)
            {
                return await FailJobAsync(job, $"AI generation failed: {generationResult.Error.Message}", cancellationToken);
            }

            var externalJobId = generationResult.Value;
            var startAIResult = job.StartAIProcessing(externalJobId);
            if (startAIResult.IsFailure)
            {
                return await FailJobAsync(job, startAIResult.Error.Message, cancellationToken);
            }
            await _jobRepository.SaveChangesAsync(cancellationToken);

            // Step 4: Ждём результат от AI
            await NotifyProgressAsync(job, VisualizationJobStatus.Processing, 50, cancellationToken);

            var aiResult = await WaitForAIResultAsync(externalJobId, job.PreferredProvider, cancellationToken);
            if (aiResult.IsFailure)
            {
                return await FailJobAsync(job, $"AI processing failed: {aiResult.Error.Message}", cancellationToken);
            }

            // Step 5: Загружаем изображения в хранилище
            await NotifyProgressAsync(job, VisualizationJobStatus.Uploading, 80, cancellationToken);

            var startUploadResult = job.StartUploading();
            if (startUploadResult.IsFailure)
            {
                return await FailJobAsync(job, startUploadResult.Error.Message, cancellationToken);
            }
            await _jobRepository.SaveChangesAsync(cancellationToken);

            foreach (var imageData in aiResult.Value.Images)
            {
                var uploadResult = await UploadImageAsync(job, imageData, cancellationToken);
                if (uploadResult.IsFailure)
                {
                    _logger.LogWarning("Failed to upload image: {Error}", uploadResult.Error.Message);
                    continue;
                }

                var addImageResult = job.AddImage(uploadResult.Value, externalJobId);
                if (addImageResult.IsFailure)
                {
                    _logger.LogWarning("Failed to add image to job: {Error}", addImageResult.Error.Message);
                }
            }

            // Step 6: Завершаем
            var completeResult = job.Complete();
            if (completeResult.IsFailure)
            {
                return await FailJobAsync(job, completeResult.Error.Message, cancellationToken);
            }

            await _jobRepository.SaveChangesAsync(cancellationToken);

            // Обновляем статус страницы в Catalog.API
            if (job.PageId.HasValue && job.SelectedImage != null)
            {
                await _catalogService.UpdatePageVisualizationStatusAsync(
                    job.PageId.Value,
                    true,
                    job.SelectedImage.ImageUrl,
                    cancellationToken);
            }

            // Уведомляем о завершении
            var dto = _mapper.Map<VisualizationJobDto>(job);
            await _notificationService.NotifyJobCompletedAsync(job.UserId, dto, cancellationToken);

            // Инвалидируем кэш
            await _cacheService.InvalidateJobAsync(jobId, cancellationToken);

            _logger.LogInformation(
                "Visualization job {JobId} completed successfully with {ImageCount} images",
                request.JobId, job.Images.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing job {JobId}", request.JobId);
            return await FailJobAsync(job, ex.Message, cancellationToken);
        }
    }

    private async Task<string?> GetOriginalTextAsync(
        Domain.Aggregates.VisualizationJobAggregate.VisualizationJob job,
        CancellationToken cancellationToken)
    {
        // Если есть выделенный текст - используем его
        if (job.TextSelection != null)
        {
            return job.TextSelection.FullContext;
        }

        // Иначе получаем текст страницы
        if (job.PageId.HasValue)
        {
            var pageResult = await _catalogService.GetPageContentAsync(job.PageId.Value, cancellationToken);
            if (pageResult.IsSuccess)
            {
                return pageResult.Value.Content;
            }
        }

        return null;
    }

    private async Task<Result<AIGenerationResultDto>> WaitForAIResultAsync(
        string externalJobId,
        AIModelProvider provider,
        CancellationToken cancellationToken)
    {
        var maxAttempts = 60; // 5 minutes with 5 second intervals
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var statusResult = await _aiGeneratorService.GetGenerationStatusAsync(
                externalJobId, provider, cancellationToken);

            if (statusResult.IsFailure)
            {
                return Result<AIGenerationResultDto>.Failure(statusResult.Error);
            }

            var status = statusResult.Value;

            switch (status.State)
            {
                case AIGenerationState.Completed:
                    return await _aiGeneratorService.GetGenerationResultAsync(
                        externalJobId, provider, cancellationToken);

                case AIGenerationState.Failed:
                    return Result<AIGenerationResultDto>.Failure(
                        Error.Failure($"AI generation failed: {status.Message}"));

                case AIGenerationState.Cancelled:
                    return Result<AIGenerationResultDto>.Failure(
                        Error.Failure("AI generation was cancelled"));

                default:
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    attempt++;
                    break;
            }
        }

        return Result<AIGenerationResultDto>.Failure(
            Error.Failure("AI generation timed out"));
    }

    private async Task<Result<ImageMetadata>> UploadImageAsync(
        Domain.Aggregates.VisualizationJobAggregate.VisualizationJob job,
        GeneratedImageDataDto imageData,
        CancellationToken cancellationToken)
    {
        var fileName = $"{job.Id.Value}_{Guid.NewGuid():N}.{imageData.Format}";
        var format = ImageFormat.FromExtension(imageData.Format);

        if (imageData.ImageData != null)
        {
            return await _storageService.UploadImageAsync(
                imageData.ImageData,
                fileName,
                format,
                job.BookId,
                cancellationToken);
        }

        if (!string.IsNullOrEmpty(imageData.ImageUrl))
        {
            return await _storageService.UploadImageFromUrlAsync(
                imageData.ImageUrl,
                fileName,
                job.BookId,
                cancellationToken);
        }

        return Result<ImageMetadata>.Failure(Error.Validation("No image data or URL provided"));
    }

    private async Task<Result<bool>> FailJobAsync(
        Domain.Aggregates.VisualizationJobAggregate.VisualizationJob job,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning("Job {JobId} failed: {Error}", job.Id.Value, errorMessage);

        job.Fail(errorMessage);
        await _jobRepository.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyJobFailedAsync(
            job.UserId,
            job.Id.Value,
            errorMessage,
            cancellationToken);

        await _cacheService.InvalidateJobAsync(job.Id, cancellationToken);

        return Result<bool>.Failure(Error.Failure(errorMessage));
    }

    private async Task NotifyProgressAsync(
        Domain.Aggregates.VisualizationJobAggregate.VisualizationJob job,
        VisualizationJobStatus status,
        int progressPercent,
        CancellationToken cancellationToken)
    {
        var progress = new JobProgressDto
        {
            JobId = job.Id.Value,
            Status = status.Name,
            StatusDisplayName = status.DisplayName,
            ProgressPercent = progressPercent,
            Timestamp = DateTime.UtcNow
        };

        await _notificationService.NotifyJobProgressAsync(job.UserId, progress, cancellationToken);
    }
}
