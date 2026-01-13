namespace NovelVision.Services.Visualization.Application.DTOs;

/// <summary>
/// DTO для сгенерированного изображения
/// </summary>
public sealed record GeneratedImageDto
{
    public Guid Id { get; init; }
    public Guid JobId { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string AspectRatio { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string FileSizeFormatted { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string ProviderDisplayName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public bool IsSelected { get; init; }
    public PromptDataDto? PromptData { get; init; }
}

/// <summary>
/// Краткая информация об изображении
/// </summary>
public sealed record GeneratedImageSummaryDto
{
    public Guid Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public bool IsSelected { get; init; }
}
