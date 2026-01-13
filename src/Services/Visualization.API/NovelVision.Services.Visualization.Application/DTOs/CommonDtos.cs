namespace NovelVision.Services.Visualization.Application.DTOs;

/// <summary>
/// DTO для выделенного текста
/// </summary>
public sealed record TextSelectionDto
{
    public string SelectedText { get; init; } = string.Empty;
    public int StartPosition { get; init; }
    public int EndPosition { get; init; }
    public Guid PageId { get; init; }
    public Guid? ChapterId { get; init; }
    public string? ContextBefore { get; init; }
    public string? ContextAfter { get; init; }
    public int Length { get; init; }
}

/// <summary>
/// DTO для параметров генерации
/// </summary>
public sealed record GenerationParametersDto
{
    public string Size { get; init; } = "1024x1024";
    public string Quality { get; init; } = "standard";
    public string? AspectRatio { get; init; }
    public int? Seed { get; init; }
    public int? Steps { get; init; }
    public double? CfgScale { get; init; }
    public string? Sampler { get; init; }
    public bool Upscale { get; init; }
}

/// <summary>
/// DTO для статуса очереди
/// </summary>
public sealed record QueueStatusDto
{
    public int TotalInQueue { get; init; }
    public int Position { get; init; }
    public TimeSpan EstimatedWaitTime { get; init; }
    public int ProcessingCount { get; init; }
    public int PendingCount { get; init; }
    public double AverageProcessingTimeSeconds { get; init; }
}

/// <summary>
/// DTO для прогресса задания (для SignalR)
/// </summary>
public sealed record JobProgressDto
{
    public Guid JobId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StatusDisplayName { get; init; } = string.Empty;
    public int ProgressPercent { get; init; }
    public string? Message { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
