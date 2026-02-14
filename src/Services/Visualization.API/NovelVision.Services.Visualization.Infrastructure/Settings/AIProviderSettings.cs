// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Settings/AIProviderSettings.cs

namespace NovelVision.Services.Visualization.Infrastructure.Settings;

/// <summary>
/// Настройки AI провайдеров
/// </summary>
public sealed class AIProviderSettings
{
    /// <summary>
    /// OpenAI (DALL-E 3)
    /// </summary>
    public OpenAISettings OpenAI { get; set; } = new();

    /// <summary>
    /// Stable Diffusion
    /// </summary>
    public StableDiffusionSettings StableDiffusion { get; set; } = new();

    /// <summary>
    /// Midjourney
    /// </summary>
    public MidjourneySettings Midjourney { get; set; } = new();

    /// <summary>
    /// Flux
    /// </summary>
    public FluxSettings Flux { get; set; } = new();

    /// <summary>
    /// Провайдер по умолчанию
    /// </summary>
    public string DefaultProvider { get; set; } = "dalle3";
}

public sealed class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "dall-e-3";
    public string Organization { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 120;
}

public sealed class StableDiffusionSettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "stable-diffusion-xl-1024-v1-0";
    public int TimeoutSeconds { get; set; } = 60;
}

public sealed class MidjourneySettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 180;
}

public sealed class FluxSettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "flux-1.1-pro";
    public int TimeoutSeconds { get; set; } = 60;
}