// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/AIProviders/StableDiffusionService.cs

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.ValueObjects;
using NovelVision.Services.Visualization.Infrastructure.Settings;

namespace NovelVision.Services.Visualization.Infrastructure.Services.AIProviders;

/// <summary>
/// Сервис генерации изображений через Stable Diffusion API
/// (Stability AI или локальный Automatic1111)
/// </summary>
public sealed class StableDiffusionService
{
    private readonly HttpClient _httpClient;
    private readonly StableDiffusionSettings _settings;
    private readonly ILogger<StableDiffusionService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public StableDiffusionService(
        IHttpClientFactory httpClientFactory,
        IOptions<AIProviderSettings> settings,
        ILogger<StableDiffusionService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("StableDiffusion");
        _settings = settings.Value.StableDiffusion;
        _logger = logger;

        if (!string.IsNullOrEmpty(_settings.ApiUrl))
        {
            _httpClient.BaseAddress = new Uri(_settings.ApiUrl);
        }

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        }
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
                "Starting Stable Diffusion generation. Prompt length: {Length}",
                prompt.Length);

            var request = new
            {
                prompt = prompt,
                negative_prompt = negativePrompt ?? "blurry, low quality, distorted, deformed",
                steps = parameters.Steps ?? 30,
                cfg_scale = parameters.CfgScale ?? 7.5,
                sampler_name = parameters.Sampler ?? "DPM++ 2M Karras",
                seed = parameters.Seed ?? -1,
                width = GetWidth(parameters.Size),
                height = GetHeight(parameters.Size)
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/sdapi/v1/txt2img",
                request,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result<string>.Failure(Error.Failure($"Stable Diffusion error: {error}"));
            }

            var result = await response.Content.ReadFromJsonAsync<SDResponse>(
                JsonOptions, cancellationToken);

            if (result?.Images == null || result.Images.Count == 0)
            {
                return Result<string>.Failure(Error.Failure("Stable Diffusion returned no images"));
            }

            // SD возвращает base64 изображения напрямую
            // Генерируем уникальный ID для отслеживания
            var jobId = $"sd_{Guid.NewGuid():N}";

            // Сохраняем результат в памяти или кэше для последующего получения
            // В реальном приложении это может быть Redis
            SDResultCache.Set(jobId, result.Images[0]);

            _logger.LogInformation("Stable Diffusion generation completed. JobId: {JobId}", jobId);

            return Result<string>.Success(jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stable Diffusion generation failed");
            return Result<string>.Failure(Error.Failure($"Stable Diffusion error: {ex.Message}"));
        }
    }

    public Task<Result<AIGenerationStatusDto>> GetGenerationStatusAsync(
        string externalJobId,
        CancellationToken cancellationToken = default)
    {
        // SD API синхронный
        var hasResult = SDResultCache.Contains(externalJobId);

        var status = new AIGenerationStatusDto
        {
            ExternalJobId = externalJobId,
            State = hasResult ? AIGenerationState.Completed : AIGenerationState.Failed,
            ProgressPercent = hasResult ? 100 : 0
        };

        return Task.FromResult(Result<AIGenerationStatusDto>.Success(status));
    }

    public Task<Result<AIGenerationResultDto>> GetGenerationResultAsync(
        string externalJobId,
        CancellationToken cancellationToken = default)
    {
        var base64Image = SDResultCache.Get(externalJobId);

        if (string.IsNullOrEmpty(base64Image))
        {
            return Task.FromResult(Result<AIGenerationResultDto>.Failure(
                Error.NotFound("Image not found")));
        }

        var imageData = Convert.FromBase64String(base64Image);

        var result = new AIGenerationResultDto
        {
            ExternalJobId = externalJobId,
            IsSuccess = true,
            Images = new List<GeneratedImageDataDto>
            {
                new GeneratedImageDataDto
                {
                    ImageData = imageData,
                    Format = "png",
                    Width = 512,
                    Height = 512
                }
            }
        };

        // Очищаем кэш
        SDResultCache.Remove(externalJobId);

        return Task.FromResult(Result<AIGenerationResultDto>.Success(result));
    }

    public Task<Result<bool>> CancelGenerationAsync(
        string externalJobId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Success(false));
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_settings.ApiUrl))
                return false;

            var response = await _httpClient.GetAsync("/sdapi/v1/sd-models", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    #region Helpers

    private static int GetWidth(string size)
    {
        var parts = size.Split('x');
        return parts.Length > 0 && int.TryParse(parts[0], out var w) ? w : 512;
    }

    private static int GetHeight(string size)
    {
        var parts = size.Split('x');
        return parts.Length > 1 && int.TryParse(parts[1], out var h) ? h : 512;
    }

    private sealed record SDResponse
    {
        public List<string> Images { get; init; } = new();
        public Dictionary<string, object>? Parameters { get; init; }
    }

    /// <summary>
    /// Простой in-memory кэш для результатов SD (в production использовать Redis)
    /// </summary>
    private static class SDResultCache
    {
        private static readonly Dictionary<string, string> _cache = new();
        private static readonly object _lock = new();

        public static void Set(string key, string value)
        {
            lock (_lock) { _cache[key] = value; }
        }

        public static string? Get(string key)
        {
            lock (_lock) { return _cache.TryGetValue(key, out var v) ? v : null; }
        }

        public static bool Contains(string key)
        {
            lock (_lock) { return _cache.ContainsKey(key); }
        }

        public static void Remove(string key)
        {
            lock (_lock) { _cache.Remove(key); }
        }
    }

    #endregion
}