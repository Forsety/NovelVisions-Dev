using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Visualization.Domain.Enums;

namespace NovelVision.Services.Visualization.Domain.ValueObjects;

/// <summary>
/// Данные промпта для генерации изображения
/// Получается от PromptGen.API
/// </summary>
public sealed class PromptData : ValueObject
{
    private PromptData() { }

    private PromptData(
        string originalText,
        string enhancedPrompt,
        string? negativePrompt,
        AIModelProvider targetModel,
        string? style,
        Dictionary<string, object>? parameters)
    {
        OriginalText = originalText;
        EnhancedPrompt = enhancedPrompt;
        NegativePrompt = negativePrompt;
        TargetModel = targetModel;
        Style = style;
        Parameters = parameters ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Оригинальный текст из книги
    /// </summary>
    public string OriginalText { get; private init; } = string.Empty;

    /// <summary>
    /// Улучшенный промпт от PromptGen.API
    /// </summary>
    public string EnhancedPrompt { get; private init; } = string.Empty;

    /// <summary>
    /// Негативный промпт (для SD, Midjourney)
    /// </summary>
    public string? NegativePrompt { get; private init; }

    /// <summary>
    /// Целевая AI модель
    /// </summary>
    public AIModelProvider TargetModel { get; private init; } = AIModelProvider.DallE3;

    /// <summary>
    /// Стиль изображения
    /// </summary>
    public string? Style { get; private init; }

    /// <summary>
    /// Дополнительные параметры для модели
    /// </summary>
    public Dictionary<string, object> Parameters { get; private init; } = new();

    public static PromptData Create(
        string originalText,
        string enhancedPrompt,
        AIModelProvider targetModel,
        string? negativePrompt = null,
        string? style = null,
        Dictionary<string, object>? parameters = null)
    {
        Guard.Against.NullOrWhiteSpace(originalText, nameof(originalText));
        Guard.Against.NullOrWhiteSpace(enhancedPrompt, nameof(enhancedPrompt));
        Guard.Against.Null(targetModel, nameof(targetModel));

        // Validate prompt length for target model
        if (enhancedPrompt.Length > targetModel.MaxPromptLength)
        {
            enhancedPrompt = enhancedPrompt[..targetModel.MaxPromptLength];
        }

        return new PromptData(
            originalText,
            enhancedPrompt,
            negativePrompt,
            targetModel,
            style,
            parameters);
    }

    /// <summary>
    /// Создать PromptData из ответа PromptGen.API
    /// </summary>
    public static PromptData FromPromptGenResponse(
        string originalText,
        string enhancedPrompt,
        string modelName,
        string? negativePrompt = null,
        string? style = null,
        Dictionary<string, object>? parameters = null)
    {
        var model = AIModelProvider.FromApiName(modelName);
        return Create(originalText, enhancedPrompt, model, negativePrompt, style, parameters);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OriginalText;
        yield return EnhancedPrompt;
        yield return NegativePrompt;
        yield return TargetModel;
        yield return Style;
    }
}
