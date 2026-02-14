// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/Import/ImportOptions.cs
// ИСПРАВЛЕНИЕ: Добавлено свойство MaxWordsPerPage
using System;

namespace NovelVision.Services.Catalog.Application.DTOs.Import;

/// <summary>
/// Опции импорта книг
/// </summary>
public sealed record ImportOptions
{
    /// <summary>
    /// Импортировать полный текст книги
    /// </summary>
    public bool ImportFullText { get; init; } = true;

    /// <summary>
    /// Парсить главы из текста
    /// </summary>
    public bool ParseChapters { get; init; } = true;

    /// <summary>
    /// Пропускать уже существующие книги
    /// </summary>
    public bool SkipExisting { get; init; } = true;

    /// <summary>
    /// Скачивать обложку
    /// </summary>
    public bool DownloadCover { get; init; } = true;

    /// <summary>
    /// Создавать автора если не существует
    /// </summary>
    public bool CreateAuthorIfNotExists { get; init; } = true;

    /// <summary>
    /// Создавать темы/категории если не существуют
    /// </summary>
    public bool CreateSubjectsIfNotExist { get; init; } = true;

    /// <summary>
    /// Количество слов на страницу (для разбиения текста)
    /// </summary>
    public int WordsPerPage { get; init; } = 300;

    /// <summary>
    /// Максимальное количество слов на страницу
    /// </summary>
    public int MaxWordsPerPage { get; init; } = 500;

    /// <summary>
    /// Минимальное количество слов на страницу
    /// </summary>
    public int MinWordsPerPage { get; init; } = 100;

    /// <summary>
    /// Язык для импорта (null = любой)
    /// </summary>
    public string? PreferredLanguage { get; init; }

    /// <summary>
    /// Таймаут загрузки в секундах
    /// </summary>
    public int DownloadTimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Максимальное количество попыток загрузки
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Автоматически публиковать после импорта
    /// </summary>
    public bool AutoPublish { get; init; } = false;

    /// <summary>
    /// Включить визуализацию после импорта
    /// </summary>
    public bool EnableVisualization { get; init; } = false;

    /// <summary>
    /// Режим визуализации по умолчанию
    /// </summary>
    public string DefaultVisualizationMode { get; init; } = "None";

    /// <summary>
    /// Опции по умолчанию
    /// </summary>
    public static ImportOptions Default => new();

    /// <summary>
    /// Опции для быстрого импорта (без полного текста)
    /// </summary>
    public static ImportOptions QuickImport => new()
    {
        ImportFullText = false,
        ParseChapters = false,
        DownloadCover = true,
        CreateAuthorIfNotExists = true
    };

    /// <summary>
    /// Опции для полного импорта
    /// </summary>
    public static ImportOptions FullImport => new()
    {
        ImportFullText = true,
        ParseChapters = true,
        DownloadCover = true,
        CreateAuthorIfNotExists = true,
        CreateSubjectsIfNotExist = true,
        WordsPerPage = 300,
        MaxWordsPerPage = 500
    };
}