// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/VisualizationSettingsDto.cs
// ИСПРАВЛЕНИЕ: Свойства сделаны settable (не init-only) для возможности присвоения в handlers
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO настроек визуализации книги
/// </summary>
public class VisualizationSettingsDto
{
    /// <summary>
    /// Режим визуализации (None, PerPage, PerChapter, UserSelected, AuthorDefined)
    /// </summary>
    public string Mode { get; set; } = "None";

    /// <summary>
    /// Алиас для Mode (для обратной совместимости)
    /// </summary>
    public string PrimaryMode
    {
        get => Mode;
        set => Mode = value;
    }

    /// <summary>
    /// Разрешён ли выбор режима читателем
    /// </summary>
    public bool AllowReaderChoice { get; set; }

    /// <summary>
    /// Доступные режимы для читателя
    /// </summary>
    public List<string> AllowedModes { get; set; } = new();

    /// <summary>
    /// Предпочтительный стиль изображений
    /// </summary>
    public string? PreferredStyle { get; set; }

    /// <summary>
    /// Предпочтительный AI провайдер
    /// </summary>
    public string? PreferredProvider { get; set; }

    /// <summary>
    /// Максимум изображений на страницу
    /// </summary>
    public int MaxImagesPerPage { get; set; } = 1;

    /// <summary>
    /// Автоматическая генерация при публикации
    /// </summary>
    public bool AutoGenerateOnPublish { get; set; }

    /// <summary>
    /// Визуализация включена
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Пустые настройки
    /// </summary>
    public static VisualizationSettingsDto Empty => new()
    {
        Mode = "None",
        AllowReaderChoice = false,
        AllowedModes = new List<string>(),
        MaxImagesPerPage = 1,
        AutoGenerateOnPublish = false,
        IsEnabled = false
    };
}