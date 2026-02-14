// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/BookStatisticsDto.cs
// ИСПРАВЛЕНИЕ: Объединены все необходимые свойства из разных использований
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO статистики книги
/// </summary>
public record BookStatisticsDto
{
    #region Content Statistics (используется в BookService)

    /// <summary>
    /// Количество глав
    /// </summary>
    public int ChapterCount { get; init; }

    /// <summary>
    /// Количество страниц
    /// </summary>
    public int PageCount { get; init; }

    /// <summary>
    /// Количество слов
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Примерное время чтения (форматированное)
    /// </summary>
    public string EstimatedReadingTime { get; init; } = string.Empty;

    /// <summary>
    /// Среднее количество слов на страницу
    /// </summary>
    public double AverageWordsPerPage { get; init; }

    /// <summary>
    /// Среднее количество слов на главу
    /// </summary>
    public double AverageWordsPerChapter { get; init; }

    /// <summary>
    /// Статистика по главам
    /// </summary>
    public List<ChapterStatisticsDto> ChapterStatistics { get; init; } = new();

    #endregion

    #region Engagement Statistics (исходные свойства)

    /// <summary>
    /// Количество скачиваний
    /// </summary>
    public int DownloadCount { get; init; }

    /// <summary>
    /// Количество просмотров
    /// </summary>
    public int ViewCount { get; init; }

    /// <summary>
    /// Средний рейтинг
    /// </summary>
    public decimal AverageRating { get; init; }

    /// <summary>
    /// Количество отзывов
    /// </summary>
    public int ReviewCount { get; init; }

    /// <summary>
    /// Количество добавлений в избранное
    /// </summary>
    public int FavoriteCount { get; init; }

    /// <summary>
    /// Количество завершённых прочтений
    /// </summary>
    public int CompletedReadCount { get; init; }

    /// <summary>
    /// Количество визуализаций
    /// </summary>
    public int VisualizationCount { get; init; }

    /// <summary>
    /// Показатель популярности
    /// </summary>
    public double PopularityScore { get; init; }

    /// <summary>
    /// Уверенность в рейтинге (достаточно ли отзывов)
    /// </summary>
    public string RatingConfidence { get; init; } = string.Empty;

    #endregion
}

/// <summary>
/// DTO статистики главы
/// </summary>
public record ChapterStatisticsDto
{
    /// <summary>
    /// Название главы
    /// </summary>
    public string ChapterTitle { get; init; } = string.Empty;

    /// <summary>
    /// Количество слов в главе
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Количество страниц в главе
    /// </summary>
    public int PageCount { get; init; }

    /// <summary>
    /// Примерное время чтения в минутах
    /// </summary>
    public double ReadingTimeMinutes { get; init; }
}