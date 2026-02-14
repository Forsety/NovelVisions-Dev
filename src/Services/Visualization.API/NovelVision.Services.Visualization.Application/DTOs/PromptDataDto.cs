namespace NovelVision.Services.Visualization.Application.DTOs;

/// <summary>
/// DTO для данных промпта
/// </summary>
public sealed record PromptDataDto
{
    public string OriginalText { get; init; } = string.Empty;
    public string EnhancedPrompt { get; init; } = string.Empty;
    public string? NegativePrompt { get; init; }
    public string TargetModel { get; init; } = string.Empty;
    public string? Style { get; init; }
    public Dictionary<string, object> Parameters { get; init; } = new();
}


