// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/BookListDto.cs
// ИСПРАВЛЕНИЕ: Добавлены все отсутствующие свойства
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO книги для списков (краткая информация)
/// </summary>
public sealed record BookListDto
{
    #region Core Properties

    /// <summary>
    /// ID книги
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Название книги
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Подзаголовок
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Краткое описание
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Язык книги
    /// </summary>
    public string Language { get; init; } = "en";

    #endregion

    #region Cover

    /// <summary>
    /// URL обложки
    /// </summary>
    public string? CoverImageUrl { get; init; }

    /// <summary>
    /// URL миниатюры
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Есть ли обложка
    /// </summary>
    public bool HasCover => !string.IsNullOrEmpty(CoverImageUrl);

    #endregion

    #region Author

    /// <summary>
    /// ID автора
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary>
    /// Имя автора
    /// </summary>
    public string AuthorName { get; init; } = string.Empty;

    #endregion

    #region Status

    /// <summary>
    /// Статус книги
    /// </summary>
    public string Status { get; init; } = "Draft";

    /// <summary>
    /// Опубликована ли книга
    /// </summary>
    public bool IsPublished { get; init; }

    /// <summary>
    /// Статус авторских прав
    /// </summary>
    public string CopyrightStatus { get; init; } = "Unknown";

    /// <summary>
    /// Бесплатна для использования
    /// </summary>
    public bool IsFree { get; init; }

    /// <summary>
    /// Бесплатна для использования (алиас)
    /// </summary>
    public bool IsFreeToUse => IsFree;

    #endregion

    #region Content Stats

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
    /// Примерное время чтения в минутах
    /// </summary>
    public int ReadingTimeMinutes { get; init; }

    #endregion

    #region Categories

    /// <summary>
    /// Жанры
    /// </summary>
    public List<string> Genres { get; init; } = new();

    /// <summary>
    /// Теги
    /// </summary>
    public List<string> Tags { get; init; } = new();

    #endregion

    #region Ratings & Statistics

    /// <summary>
    /// Средний рейтинг
    /// </summary>
    public decimal Rating { get; init; }

    /// <summary>
    /// Средний рейтинг (алиас)
    /// </summary>
    public decimal AverageRating => Rating;

    /// <summary>
    /// Количество отзывов
    /// </summary>
    public int ReviewCount { get; init; }

    /// <summary>
    /// Количество скачиваний
    /// </summary>
    public int DownloadCount { get; init; }

    /// <summary>
    /// Количество просмотров
    /// </summary>
    public int ViewCount { get; init; }

    #endregion

    #region Visualization

    /// <summary>
    /// Включена ли визуализация
    /// </summary>
    public bool HasVisualization { get; init; }

    /// <summary>
    /// Режим визуализации
    /// </summary>
    public string VisualizationMode { get; init; } = "None";

    #endregion

    #region Source

    /// <summary>
    /// Источник книги
    /// </summary>
    public string Source { get; init; } = "UserCreated";

    /// <summary>
    /// Из внешнего источника
    /// </summary>
    public bool IsImported { get; init; }

    #endregion

    #region Timestamps

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата обновления
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Дата публикации
    /// </summary>
    public DateTime? PublishedAt { get; init; }

    #endregion
}
/// <summary>
/// DTO для карточки книги (минимум данных)
/// </summary>
public record BookCardDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string AuthorName { get; init; } = string.Empty;
    public string? CoverImageUrl { get; init; }
    public string Language { get; init; } = string.Empty;
    public List<string> Genres { get; init; } = new();
    public bool IsFreeToUse { get; init; }
    public int DownloadCount { get; init; }
}