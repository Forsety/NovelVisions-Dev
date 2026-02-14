// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/PagesForVisualizationDto.cs
// НОВЫЙ ФАЙЛ: DTOs для запросов визуализации страниц
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO страниц для визуализации
/// </summary>
public sealed record PagesForVisualizationDto
{
    /// <summary>
    /// ID книги
    /// </summary>
    public Guid BookId { get; init; }

    /// <summary>
    /// Название книги
    /// </summary>
    public string BookTitle { get; init; } = string.Empty;

    /// <summary>
    /// Автор книги
    /// </summary>
    public string AuthorName { get; init; } = string.Empty;

    /// <summary>
    /// Режим визуализации
    /// </summary>
    public string VisualizationMode { get; init; } = "None";

    /// <summary>
    /// Страницы с информацией о визуализации
    /// </summary>
    public List<PageVisualizationInfoDto> Pages { get; init; } = new();

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Количество страниц с визуализацией
    /// </summary>
    public int PagesWithVisualization { get; init; }

    /// <summary>
    /// Количество страниц без визуализации
    /// </summary>
    public int PagesWithoutVisualization { get; init; }

    /// <summary>
    /// Количество страниц ожидающих визуализации
    /// </summary>
    public int PagesPendingVisualization { get; init; }

    /// <summary>
    /// Процент завершённых визуализаций
    /// </summary>
    public double CompletionPercentage => TotalPages > 0
        ? (double)PagesWithVisualization / TotalPages * 100
        : 0;
}

/// <summary>
/// DTO с информацией о визуализации страницы
/// </summary>
public sealed record PageVisualizationInfoDto
{
    /// <summary>
    /// ID страницы
    /// </summary>
    public Guid PageId { get; init; }

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
    /// Глобальный номер страницы в книге
    /// </summary>
    public int GlobalPageNumber { get; init; }

    /// <summary>
    /// Первые N символов контента (превью)
    /// </summary>
    public string ContentPreview { get; init; } = string.Empty;

    /// <summary>
    /// Количество слов
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Статус визуализации
    /// </summary>
    public PageVisualizationStatus VisualizationStatus { get; init; } = PageVisualizationStatus.None;

    /// <summary>
    /// URL изображения визуализации
    /// </summary>
    public string? VisualizationImageUrl { get; init; }

    /// <summary>
    /// URL миниатюры
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// ID задания визуализации
    /// </summary>
    public Guid? VisualizationJobId { get; init; }

    /// <summary>
    /// Помечена как точка визуализации
    /// </summary>
    public bool IsVisualizationPoint { get; init; }

    /// <summary>
    /// Подсказка автора для визуализации
    /// </summary>
    public string? AuthorHint { get; init; }

    /// <summary>
    /// Дата генерации визуализации
    /// </summary>
    public DateTime? GeneratedAt { get; init; }

    /// <summary>
    /// Сообщение об ошибке (если была)
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Есть ли визуализация
    /// </summary>
    public bool HasVisualization => VisualizationStatus == PageVisualizationStatus.Completed &&
                                    !string.IsNullOrEmpty(VisualizationImageUrl);

    /// <summary>
    /// Можно ли запустить визуализацию
    /// </summary>
    public bool CanVisualize => VisualizationStatus == PageVisualizationStatus.None ||
                                 VisualizationStatus == PageVisualizationStatus.Failed;
}

/// <summary>
/// Запрос на визуализацию страницы
/// </summary>
public sealed record PageVisualizationRequestDto
{
    /// <summary>
    /// ID страницы
    /// </summary>
    public Guid PageId { get; init; }

    /// <summary>
    /// Кастомный промпт (опционально)
    /// </summary>
    public string? CustomPrompt { get; init; }

    /// <summary>
    /// Предпочтительный стиль
    /// </summary>
    public string? PreferredStyle { get; init; }

    /// <summary>
    /// Предпочтительный провайдер
    /// </summary>
    public string? PreferredProvider { get; init; }

    /// <summary>
    /// Приоритет задания
    /// </summary>
    public int Priority { get; init; } = 5;
}

/// <summary>
/// Результат запуска визуализации
/// </summary>
public sealed record VisualizationJobResultDto
{
    /// <summary>
    /// ID задания
    /// </summary>
    public Guid JobId { get; init; }

    /// <summary>
    /// ID страницы
    /// </summary>
    public Guid PageId { get; init; }

    /// <summary>
    /// Статус
    /// </summary>
    public string Status { get; init; } = "Queued";

    /// <summary>
    /// Позиция в очереди
    /// </summary>
    public int? QueuePosition { get; init; }

    /// <summary>
    /// Примерное время ожидания
    /// </summary>
    public TimeSpan? EstimatedWaitTime { get; init; }
}