using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Application.Interfaces;

/// <summary>
/// Интерфейс для генерации изображений через AI провайдеров
/// </summary>
public interface IAIImageGeneratorService
{
    /// <summary>
    /// Запустить генерацию изображения
    /// </summary>
    /// <param name="prompt">Промпт для генерации</param>
    /// <param name="negativePrompt">Негативный промпт (опционально)</param>
    /// <param name="provider">AI провайдер</param>
    /// <param name="parameters">Параметры генерации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>External Job ID для отслеживания</returns>
    Task<Result<string>> StartGenerationAsync(
        string prompt,
        string? negativePrompt,
        AIModelProvider provider,
        GenerationParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить статус генерации
    /// </summary>
    Task<Result<AIGenerationStatusDto>> GetGenerationStatusAsync(
        string externalJobId,
        AIModelProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить результат генерации
    /// </summary>
    Task<Result<AIGenerationResultDto>> GetGenerationResultAsync(
        string externalJobId,
        AIModelProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отменить генерацию
    /// </summary>
    Task<Result<bool>> CancelGenerationAsync(
        string externalJobId,
        AIModelProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить доступность провайдера
    /// </summary>
    Task<bool> IsProviderAvailableAsync(
        AIModelProvider provider,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO статуса генерации от AI провайдера
/// </summary>
public sealed record AIGenerationStatusDto
{
    public string ExternalJobId { get; init; } = string.Empty;
    public AIGenerationState State { get; init; }
    public int? ProgressPercent { get; init; }
    public string? Message { get; init; }
    public DateTime? EstimatedCompletionTime { get; init; }
}

/// <summary>
/// DTO результата генерации
/// </summary>
public sealed record AIGenerationResultDto
{
    public string ExternalJobId { get; init; } = string.Empty;
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<GeneratedImageDataDto> Images { get; init; } = Array.Empty<GeneratedImageDataDto>();
}

/// <summary>
/// DTO данных сгенерированного изображения (от AI)
/// </summary>
public sealed record GeneratedImageDataDto
{
    public byte[]? ImageData { get; init; }
    public string? ImageUrl { get; init; }
    public string Format { get; init; } = "png";
    public int Width { get; init; }
    public int Height { get; init; }
    public string? RevisedPrompt { get; init; }
}

/// <summary>
/// Состояние генерации AI
/// </summary>
public enum AIGenerationState
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
