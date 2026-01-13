using Ardalis.SmartEnum;

namespace NovelVision.Services.Visualization.Domain.Enums;

/// <summary>
/// Провайдер AI модели для генерации изображений
/// </summary>
public sealed class AIModelProvider : SmartEnum<AIModelProvider>
{
    /// <summary>
    /// OpenAI DALL-E 3
    /// </summary>
    public static readonly AIModelProvider DallE3 = new(
        nameof(DallE3), 1, 
        "dalle3", 
        "DALL-E 3",
        maxPromptLength: 4000,
        supportsNegativePrompt: false,
        averageGenerationTimeSeconds: 15);

    /// <summary>
    /// Midjourney (через Discord API или unofficial)
    /// </summary>
    public static readonly AIModelProvider Midjourney = new(
        nameof(Midjourney), 2, 
        "midjourney", 
        "Midjourney",
        maxPromptLength: 6000,
        supportsNegativePrompt: true,
        averageGenerationTimeSeconds: 60);

    /// <summary>
    /// Stable Diffusion (self-hosted или API)
    /// </summary>
    public static readonly AIModelProvider StableDiffusion = new(
        nameof(StableDiffusion), 3, 
        "stable-diffusion", 
        "Stable Diffusion",
        maxPromptLength: 380,
        supportsNegativePrompt: true,
        averageGenerationTimeSeconds: 10);

    /// <summary>
    /// Flux модель
    /// </summary>
    public static readonly AIModelProvider Flux = new(
        nameof(Flux), 4, 
        "flux", 
        "Flux",
        maxPromptLength: 1000,
        supportsNegativePrompt: true,
        averageGenerationTimeSeconds: 20);

    private AIModelProvider(
        string name, 
        int value, 
        string apiName,
        string displayName,
        int maxPromptLength,
        bool supportsNegativePrompt,
        int averageGenerationTimeSeconds) 
        : base(name, value)
    {
        ApiName = apiName;
        DisplayName = displayName;
        MaxPromptLength = maxPromptLength;
        SupportsNegativePrompt = supportsNegativePrompt;
        AverageGenerationTimeSeconds = averageGenerationTimeSeconds;
    }

    /// <summary>
    /// Имя модели для API вызовов (совпадает с PromptGen.API)
    /// </summary>
    public string ApiName { get; }

    /// <summary>
    /// Отображаемое имя
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Максимальная длина промпта
    /// </summary>
    public int MaxPromptLength { get; }

    /// <summary>
    /// Поддерживает ли негативный промпт
    /// </summary>
    public bool SupportsNegativePrompt { get; }

    /// <summary>
    /// Среднее время генерации в секундах
    /// </summary>
    public int AverageGenerationTimeSeconds { get; }

    /// <summary>
    /// Получить провайдера по API имени
    /// </summary>
    public static AIModelProvider FromApiName(string apiName)
    {
        return List.FirstOrDefault(p => 
            p.ApiName.Equals(apiName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"AI provider with API name '{apiName}' not found");
    }
}
