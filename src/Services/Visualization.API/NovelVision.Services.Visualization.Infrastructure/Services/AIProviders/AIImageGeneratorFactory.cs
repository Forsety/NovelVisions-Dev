// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/AIProviders/AIImageGeneratorFactory.cs

using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Infrastructure.Services.AIProviders;

/// <summary>
/// Factory для выбора AI провайдера генерации изображений
/// Реализует IAIImageGeneratorService и делегирует к конкретным провайдерам
/// </summary>
public sealed class AIImageGeneratorFactory : IAIImageGeneratorService
{
    private readonly DallE3Service _dallE3Service;
    private readonly StableDiffusionService _stableDiffusionService;
    private readonly ILogger<AIImageGeneratorFactory> _logger;

    public AIImageGeneratorFactory(
        DallE3Service dallE3Service,
        StableDiffusionService stableDiffusionService,
        ILogger<AIImageGeneratorFactory> logger)
    {
        _dallE3Service = dallE3Service;
        _stableDiffusionService = stableDiffusionService;
        _logger = logger;
    }

    public async Task<Result<string>> StartGenerationAsync(
        string prompt,
        string? negativePrompt,
        AIModelProvider provider,
        GenerationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting image generation with provider: {Provider}",
            provider.Name);

        return provider.Name switch
        {
            "DallE3" => await _dallE3Service.StartGenerationAsync(
                prompt, negativePrompt, parameters, cancellationToken),

            "StableDiffusion" => await _stableDiffusionService.StartGenerationAsync(
                prompt, negativePrompt, parameters, cancellationToken),

            "Midjourney" => Result<string>.Failure(
                Error.Failure("Midjourney provider is not yet implemented")),

            "Flux" => Result<string>.Failure(
                Error.Failure("Flux provider is not yet implemented")),

            _ => Result<string>.Failure(
                Error.Failure($"Unknown AI provider: {provider.Name}"))
        };
    }

    public async Task<Result<AIGenerationStatusDto>> GetGenerationStatusAsync(
        string externalJobId,
        AIModelProvider provider,
        CancellationToken cancellationToken = default)
    {
        return provider.Name switch
        {
            "DallE3" => await _dallE3Service.GetGenerationStatusAsync(
                externalJobId, cancellationToken),

            "StableDiffusion" => await _stableDiffusionService.GetGenerationStatusAsync(
                externalJobId, cancellationToken),

            _ => Result<AIGenerationStatusDto>.Failure(
                Error.Failure($"Unknown AI provider: {provider.Name}"))
        };
    }

    public async Task<Result<AIGenerationResultDto>> GetGenerationResultAsync(
        string externalJobId,
        AIModelProvider provider,
        CancellationToken cancellationToken = default)
    {
        return provider.Name switch
        {
            "DallE3" => await _dallE3Service.GetGenerationResultAsync(
                externalJobId, cancellationToken),

            "StableDiffusion" => await _stableDiffusionService.GetGenerationResultAsync(
                externalJobId, cancellationToken),

            _ => Result<AIGenerationResultDto>.Failure(
                Error.Failure($"Unknown AI provider: {provider.Name}"))
        };
    }

    public async Task<Result<bool>> CancelGenerationAsync(
        string externalJobId,
        AIModelProvider provider,
        CancellationToken cancellationToken = default)
    {
        return provider.Name switch
        {
            "DallE3" => await _dallE3Service.CancelGenerationAsync(
                externalJobId, cancellationToken),

            "StableDiffusion" => await _stableDiffusionService.CancelGenerationAsync(
                externalJobId, cancellationToken),

            _ => Result<bool>.Failure(
                Error.Failure($"Unknown AI provider: {provider.Name}"))
        };
    }

    public async Task<bool> IsProviderAvailableAsync(
        AIModelProvider provider,
        CancellationToken cancellationToken = default)
    {
        return provider.Name switch
        {
            "DallE3" => await _dallE3Service.IsAvailableAsync(cancellationToken),
            "StableDiffusion" => await _stableDiffusionService.IsAvailableAsync(cancellationToken),
            "Midjourney" => false,
            "Flux" => false,
            _ => false
        };
    }
}