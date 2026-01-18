// src/Services/Visualization.API/NovelVision.Services.Visualization.Application/DTOs/PromptGenResponseDto.cs

namespace NovelVision.Services.Visualization.Application.DTOs;

/// <summary>
/// DTO ответа от PromptGen.API
/// </summary>
public sealed record PromptGenResponseDto
{
    /// <summary>
    /// Улучшенный промпт для AI модели
    /// </summary>
    public string EnhancedPrompt { get; init; } = string.Empty;

    /// <summary>
    /// Негативный промпт (для SD, Flux)
    /// </summary>
    public string? NegativePrompt { get; init; }

    /// <summary>
    /// Целевая модель
    /// </summary>
    public string TargetModel { get; init; } = string.Empty;

    /// <summary>
    /// Применённый стиль
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Описание сцены
    /// </summary>
    public string? SceneDescription { get; init; }

    /// <summary>
    /// Персонажи в сцене
    /// </summary>
    public IReadOnlyList<string> Characters { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Параметры для AI модели
    /// </summary>
    public IReadOnlyDictionary<string, object> Parameters { get; init; } =
        new Dictionary<string, object>();

    /// <summary>
    /// Время обработки (мс)
    /// </summary>
    public int ProcessingTimeMs { get; init; }
}

/// <summary>
/// DTO данных консистентности персонажа
/// </summary>
public sealed record CharacterConsistencyDto
{
    /// <summary>
    /// Имя персонажа
    /// </summary>
    public string CharacterName { get; init; } = string.Empty;

    /// <summary>
    /// ID книги
    /// </summary>
    public Guid BookId { get; init; }

    /// <summary>
    /// Описание внешности
    /// </summary>
    public string? Appearance { get; init; }

    /// <summary>
    /// Описание одежды
    /// </summary>
    public string? Clothing { get; init; }

    /// <summary>
    /// Отличительные черты
    /// </summary>
    public IReadOnlyList<string>? DistinguishingFeatures { get; init; }

    /// <summary>
    /// Готовый фрагмент для промпта
    /// </summary>
    public string? PromptFragment { get; init; }

    /// <summary>
    /// Персонаж уже установлен (был сгенерирован)
    /// </summary>
    public bool IsEstablished { get; init; }

    /// <summary>
    /// Количество генераций с этим персонажем
    /// </summary>
    public int GenerationCount { get; init; }
}