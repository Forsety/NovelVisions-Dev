// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/BookDetailDto.cs
// НОВЫЙ ФАЙЛ: BookDetailDto для детальной информации о книге
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO детальной информации о книге
/// </summary>
public sealed record BookDetailDto
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
    /// Оригинальное название
    /// </summary>
    public string? OriginalTitle { get; init; }

    /// <summary>
    /// Описание
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Язык книги
    /// </summary>
    public string Language { get; init; } = "en";

    #endregion

    #region Cover & Images

    /// <summary>
    /// URL обложки
    /// </summary>
    public string? CoverImageUrl { get; init; }

    /// <summary>
    /// URL миниатюры обложки
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// Есть ли обложка
    /// </summary>
    public bool HasCover => !string.IsNullOrEmpty(CoverImageUrl);

    #endregion

    #region Author Info

    /// <summary>
    /// ID автора
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary>
    /// Имя автора
    /// </summary>
    public string AuthorName { get; init; } = string.Empty;

    /// <summary>
    /// Информация об авторе
    /// </summary>
    public AuthorDto? Author { get; init; }

    #endregion

    #region Publication Info

    /// <summary>
    /// ISBN
    /// </summary>
    public string? ISBN { get; init; }

    /// <summary>
    /// Издатель
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// Дата публикации
    /// </summary>
    public DateTime? PublicationDate { get; init; }

    /// <summary>
    /// Год публикации
    /// </summary>
    public int? PublicationYear { get; init; }

    /// <summary>
    /// Оригинальный год публикации
    /// </summary>
    public int? OriginalPublicationYear { get; init; }

    /// <summary>
    /// Издание
    /// </summary>
    public string? Edition { get; init; }

    #endregion

    #region Status & Rights

    /// <summary>
    /// Статус книги
    /// </summary>
    public string Status { get; init; } = "Draft";

    /// <summary>
    /// Статус авторских прав
    /// </summary>
    public string CopyrightStatus { get; init; } = "Unknown";

    /// <summary>
    /// Бесплатна для использования
    /// </summary>
    public bool IsFreeToUse { get; init; }

    /// <summary>
    /// Опубликована ли книга
    /// </summary>
    public bool IsPublished { get; init; }

    /// <summary>
    /// Дата публикации в системе
    /// </summary>
    public DateTime? PublishedAt { get; init; }

    #endregion

    #region Content Statistics

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
    /// Примерное время чтения
    /// </summary>
    public TimeSpan EstimatedReadingTime { get; init; }

    /// <summary>
    /// Форматированное время чтения
    /// </summary>
    public string ReadingTimeFormatted => FormatReadingTime(EstimatedReadingTime);

    #endregion

    #region Categories & Tags

    /// <summary>
    /// Жанры
    /// </summary>
    public List<string> Genres { get; init; } = new();

    /// <summary>
    /// Теги
    /// </summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Темы/категории
    /// </summary>
    public List<SubjectDto> Subjects { get; init; } = new();

    #endregion

    #region External Source

    /// <summary>
    /// Источник книги
    /// </summary>
    public string Source { get; init; } = "UserCreated";

    /// <summary>
    /// Внешний источник
    /// </summary>
    public string? ExternalSource { get; init; }

    /// <summary>
    /// ID во внешнем источнике
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// URL во внешнем источнике
    /// </summary>
    public string? ExternalUrl { get; init; }

    /// <summary>
    /// Из внешнего источника
    /// </summary>
    public bool IsFromExternalSource => !string.IsNullOrEmpty(ExternalSource);

    #endregion

    #region Statistics

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

    #endregion

    #region Visualization

    /// <summary>
    /// Настройки визуализации
    /// </summary>
    public VisualizationSettingsDto? VisualizationSettings { get; init; }

    /// <summary>
    /// Включена ли визуализация
    /// </summary>
    public bool HasVisualization { get; init; }

    /// <summary>
    /// Режим визуализации
    /// </summary>
    public string VisualizationMode { get; init; } = "None";

    #endregion

    #region Chapters

    /// <summary>
    /// Главы книги
    /// </summary>
    public List<ChapterDto> Chapters { get; init; } = new();

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

    #endregion

    #region Navigation

    /// <summary>
    /// URL полного текста
    /// </summary>
    public string? FullTextUrl { get; init; }

    /// <summary>
    /// Есть ли полный текст
    /// </summary>
    public bool HasFullText { get; init; }

    #endregion

    #region Helper Methods

    private static string FormatReadingTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
        {
            var hours = (int)time.TotalHours;
            var minutes = time.Minutes;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }
        return $"{(int)time.TotalMinutes}m";
    }

    #endregion
}