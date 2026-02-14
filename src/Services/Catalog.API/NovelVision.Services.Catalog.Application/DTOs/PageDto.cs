// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/PageDto.cs
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// Статус визуализации страницы
/// </summary>
public enum PageVisualizationStatus
{
    None = 0,
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4
}

/// <summary>
/// DTO страницы с полной информацией
/// </summary>
public sealed record PageDto
{
    /// <summary>
    /// ID страницы
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID главы
    /// </summary>
    public Guid ChapterId { get; init; }

    /// <summary>
    /// Номер страницы
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Контент страницы
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Количество слов
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Количество символов
    /// </summary>
    public int CharacterCount { get; init; }

    /// <summary>
    /// Примерное время чтения в минутах
    /// </summary>
    public double EstimatedReadingTimeMinutes { get; init; }

    /// <summary>
    /// Примерное время чтения (алиас для совместимости)
    /// </summary>
    public double EstimatedReadingTime => EstimatedReadingTimeMinutes;

    /// <summary>
    /// Промпты визуализации (legacy)
    /// </summary>
    public List<string> VisualizationPrompts { get; init; } = new();

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата обновления
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    #region Visualization Properties

    /// <summary>
    /// Есть ли визуализация
    /// </summary>
    public bool HasVisualization { get; init; }

    /// <summary>
    /// URL изображения визуализации
    /// </summary>
    public string? VisualizationImageUrl { get; init; }

    /// <summary>
    /// URL сгенерированного изображения (алиас)
    /// </summary>
    public string? GeneratedImageUrl => VisualizationImageUrl;

    /// <summary>
    /// URL миниатюры
    /// </summary>
    public string? VisualizationThumbnailUrl { get; init; }

    /// <summary>
    /// URL миниатюры (алиас)
    /// </summary>
    public string? ThumbnailUrl => VisualizationThumbnailUrl;

    /// <summary>
    /// ID задания визуализации
    /// </summary>
    public Guid? VisualizationJobId { get; init; }

    /// <summary>
    /// Помечена как точка визуализации автором
    /// </summary>
    public bool IsVisualizationPoint { get; init; }

    /// <summary>
    /// Подсказка автора для визуализации
    /// </summary>
    public string? AuthorVisualizationHint { get; init; }

    /// <summary>
    /// Дата генерации визуализации
    /// </summary>
    public DateTime? VisualizationGeneratedAt { get; init; }

    /// <summary>
    /// Статус визуализации
    /// </summary>
    public PageVisualizationStatus VisualizationStatus { get; init; } = PageVisualizationStatus.None;

    #endregion
}

/// <summary>
/// DTO страницы для списков (краткая информация)
/// </summary>
public sealed record PageListDto
{
    public Guid Id { get; init; }
    public Guid ChapterId { get; init; }
    public int PageNumber { get; init; }
    public int WordCount { get; init; }
    public bool HasVisualization { get; init; }
    public bool IsVisualizationPoint { get; init; }
    public PageVisualizationStatus VisualizationStatus { get; init; } = PageVisualizationStatus.None;
}

/// <summary>
/// DTO страницы для чтения
/// </summary>
public sealed record PageForReadingDto
{
    /// <summary>
    /// ID страницы
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID главы
    /// </summary>
    public Guid ChapterId { get; init; }

    /// <summary>
    /// Название главы
    /// </summary>
    public string ChapterTitle { get; init; } = string.Empty;

    /// <summary>
    /// Номер главы
    /// </summary>
    public int ChapterNumber { get; init; }

    /// <summary>
    /// Номер страницы
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Контент страницы
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Количество слов
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Примерное время чтения в минутах
    /// </summary>
    public double EstimatedReadingTimeMinutes { get; init; }

    /// <summary>
    /// URL изображения (inline)
    /// </summary>
    public string? VisualizationImageUrl { get; init; }

    /// <summary>
    /// URL миниатюры
    /// </summary>
    public string? VisualizationThumbnailUrl { get; init; }

    /// <summary>
    /// Есть ли визуализация
    /// </summary>
    public bool HasVisualization { get; init; }

    /// <summary>
    /// Помечена как точка визуализации
    /// </summary>
    public bool IsVisualizationPoint { get; init; }

    /// <summary>
    /// Есть ли предыдущая страница
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Есть ли следующая страница
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Всего страниц в главе
    /// </summary>
    public int TotalPagesInChapter { get; init; }

    /// <summary>
    /// Всего глав в книге
    /// </summary>
    public int TotalChaptersInBook { get; init; }
}

/// <summary>
/// DTO страниц для визуализации
/// </summary>
