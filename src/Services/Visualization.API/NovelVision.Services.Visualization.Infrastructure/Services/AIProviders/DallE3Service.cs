// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/AIProviders/DallE3Service.cs

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.ValueObjects;
using NovelVision.Services.Visualization.Infrastructure.Settings;
using OpenAI;
using OpenAI.Images;

namespace NovelVision.Services.Visualization.Infrastructure.Services.AIProviders;

/// <summary>
/// Сервис генерации изображений через OpenAI DALL-E 3
/// </summary>
public sealed class DallE3Service
{
    private readonly OpenAIClient _client;
    private readonly OpenAISettings _settings;
    private readonly ILogger<DallE3Service> _logger;

    public DallE3Service(
        IOptions<AIProviderSettings> settings,
        ILogger<DallE3Service> logger)
    {
        _settings = settings.Value.OpenAI;
        _logger = logger;

        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured");
        }

        _client = new OpenAIClient(_settings.ApiKey);
    }

    public async Task<Result<string>> StartGenerationAsync(
        string prompt,
        string? negativePrompt,
        GenerationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting DALL-E 3 generation. Prompt length: {Length}, Size: {Size}",
                prompt.Length, parameters.Size);

            var imageClient = _client.GetImageClient(_settings.Model);

            // DALL-E 3 не поддерживает negative prompt напрямую
            // Можно добавить в конец основного промпта
            var finalPrompt = prompt;
            if (!string.IsNullOrEmpty(negativePrompt))
            {
                finalPrompt = $"{prompt}. Avoid: {negativePrompt}";
            }

            // Определяем размер
            var size = parameters.Size switch
            {
                "1792x1024" => GeneratedImageSize.W1792xH1024,
                "1024x1792" => GeneratedImageSize.W1024xH1792,
                _ => GeneratedImageSize.W1024xH1024
            };

            // Определяем качество
            var quality = parameters.Quality?.ToLower() switch
            {
                "hd" => GeneratedImageQuality.High,
                "high" => GeneratedImageQuality.High,
                _ => GeneratedImageQuality.Standard
            };

            var options = new ImageGenerationOptions
            {
                Size = size,
                Quality = quality,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Uri
            };

            var result = await imageClient.GenerateImageAsync(
                finalPrompt,
                options,
                cancellationToken);

            if (result?.Value == null)
            {
                return Result<string>.Failure(Error.Failure("DALL-E 3 returned empty result"));
            }

            // DALL-E 3 возвращает результат синхронно, используем URL как ID
            var imageUrl = result.Value.ImageUri?.ToString();
            if (string.IsNullOrEmpty(imageUrl))
            {
                return Result<string>.Failure(Error.Failure("DALL-E 3 returned no image URL"));
            }

            _logger.LogInformation("DALL-E 3 generation completed. URL: {Url}", imageUrl);

            // Возвращаем URL как "external job id" (DALL-E синхронный)
            return Result<string>.Success(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DALL-E 3 generation failed");
            return Result<string>.Failure(Error.Failure($"DALL-E 3 error: {ex.Message}"));
        }
    }

    public Task<Result<AIGenerationStatusDto>> GetGenerationStatusAsync(
        string externalJobId,
        CancellationToken cancellationToken = default)
    {
        // DALL-E 3 синхронный - если есть URL, значит готово
        var status = new AIGenerationStatusDto
        {
            ExternalJobId = externalJobId,
            State = AIGenerationState.Completed,
            ProgressPercent = 100,
            Message = "Image generated"
        };

        return Task.FromResult(Result<AIGenerationStatusDto>.Success(status));
    }

    public async Task<Result<AIGenerationResultDto>> GetGenerationResultAsync(
        string externalJobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // externalJobId для DALL-E - это URL изображения
            var imageUrl = externalJobId;

            // Скачиваем изображение
            using var httpClient = new HttpClient();
            var imageData = await httpClient.GetByteArrayAsync(imageUrl, cancellationToken);

            var result = new AIGenerationResultDto
            {
                ExternalJobId = externalJobId,
                IsSuccess = true,
                Images = new List<GeneratedImageDataDto>
                {
                    new GeneratedImageDataDto
                    {
                        ImageData = imageData,
                        ImageUrl = imageUrl,
                        Format = "png",
                        Width = 1024,
                        Height = 1024
                    }
                }
            };

            return Result<AIGenerationResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get DALL-E 3 result");
            return Result<AIGenerationResultDto>.Failure(Error.Failure(ex.Message));
        }
    }

    public Task<Result<bool>> CancelGenerationAsync(
        string externalJobId,
        CancellationToken cancellationToken = default)
    {
        // DALL-E 3 синхронный - отмена невозможна
        return Task.FromResult(Result<bool>.Success(false));
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!string.IsNullOrEmpty(_settings.ApiKey));
    }
}