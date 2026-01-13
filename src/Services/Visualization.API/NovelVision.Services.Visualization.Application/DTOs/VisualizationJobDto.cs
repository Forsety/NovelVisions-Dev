namespace NovelVision.Services.Visualization.Application.DTOs;

/// <summary>
/// DTO для задания визуализации
/// </summary>
public sealed record VisualizationJobDto
{
    public Guid Id { get; init; }
    public Guid BookId { get; init; }
    public Guid? PageId { get; init; }
    public Guid? ChapterId { get; init; }
    public Guid UserId { get; init; }
    public string Trigger { get; init; } = string.Empty;
    public string TriggerDisplayName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string StatusDisplayName { get; init; } = string.Empty;
    public string PreferredProvider { get; init; } = string.Empty;
    public string PreferredProviderDisplayName { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string? ErrorMessage { get; init; }
    public int RetryCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessingStartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public double? ProcessingTimeSeconds { get; init; }
    public bool CanCancel { get; init; }
    public bool CanRetry { get; init; }
    public bool HasImages { get; init; }
    public int ImageCount { get; init; }
    public PromptDataDto? PromptData { get; init; }
    public TextSelectionDto? TextSelection { get; init; }
    public GenerationParametersDto Parameters { get; init; } = new();
    public IReadOnlyList<GeneratedImageDto> Images { get; init; } = Array.Empty<GeneratedImageDto>();
    public GeneratedImageDto? SelectedImage { get; init; }
}

/// <summary>
/// Краткая информация о задании (для списков)
/// </summary>
public sealed record VisualizationJobSummaryDto
{
    public Guid Id { get; init; }
    public Guid BookId { get; init; }
    public Guid? PageId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StatusDisplayName { get; init; } = string.Empty;
    public string Trigger { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public bool HasImages { get; init; }
    public string? ThumbnailUrl { get; init; }
}
