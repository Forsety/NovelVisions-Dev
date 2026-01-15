// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/VisualizationSettings.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.Enums;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Настройки визуализации книги
/// </summary>
public sealed class VisualizationSettings : ValueObject
{
    private readonly List<VisualizationMode> _allowedModes = new();

    #region Constructors

    private VisualizationSettings()
    {
        PrimaryMode = VisualizationMode.None;
    }

    private VisualizationSettings(
        VisualizationMode primaryMode,
        bool allowReaderChoice,
        IEnumerable<VisualizationMode>? allowedModes,
        string? preferredStyle,
        string? preferredProvider,
        int maxImagesPerPage,
        bool autoGenerateOnPublish)
    {
        PrimaryMode = primaryMode;
        AllowReaderChoice = allowReaderChoice;
        _allowedModes = allowedModes?.ToList() ?? new List<VisualizationMode>();
        PreferredStyle = preferredStyle?.Trim();
        PreferredProvider = preferredProvider?.Trim().ToLowerInvariant();
        MaxImagesPerPage = Math.Max(1, Math.Min(maxImagesPerPage, 10));
        AutoGenerateOnPublish = autoGenerateOnPublish;
        IsEnabled = primaryMode != VisualizationMode.None;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Основной режим визуализации
    /// </summary>
    public VisualizationMode PrimaryMode { get; private set; } = VisualizationMode.None;

    /// <summary>
    /// Может ли читатель переключать режимы
    /// </summary>
    public bool AllowReaderChoice { get; private set; }

    /// <summary>
    /// Доступные режимы для читателя
    /// </summary>
    public IReadOnlyList<VisualizationMode> AllowedModes => _allowedModes.AsReadOnly();

    /// <summary>
    /// Backing property для EF Core - сериализация AllowedModes как строка
    /// </summary>
    public string? AllowedModesJson
    {
        get => _allowedModes.Count > 0
            ? string.Join(",", _allowedModes.Select(m => m.Value))
            : null;
        set
        {
            _allowedModes.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                var values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var v in values)
                {
                    if (int.TryParse(v.Trim(), out var intValue))
                    {
                        var mode = VisualizationMode.TryFromValue(intValue, out var result)
                            ? result
                            : null;
                        if (mode != null)
                        {
                            _allowedModes.Add(mode);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Предпочтительный стиль (manga, realistic, fantasy, etc.)
    /// </summary>
    public string? PreferredStyle { get; private set; }

    /// <summary>
    /// Предпочтительный провайдер (dalle3, midjourney, stable-diffusion)
    /// </summary>
    public string? PreferredProvider { get; private set; }

    /// <summary>
    /// Максимум изображений на страницу
    /// </summary>
    public int MaxImagesPerPage { get; private set; } = 1;

    /// <summary>
    /// Автоматически генерировать при публикации
    /// </summary>
    public bool AutoGenerateOnPublish { get; private set; }

    /// <summary>
    /// Включена ли визуализация
    /// </summary>
    public bool IsEnabled { get; private set; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Создаёт настройки визуализации
    /// </summary>
    public static VisualizationSettings Create(
        VisualizationMode primaryMode,
        bool allowReaderChoice = false,
        IEnumerable<VisualizationMode>? allowedModes = null,
        string? preferredStyle = null,
        string? preferredProvider = null,
        int maxImagesPerPage = 1,
        bool autoGenerateOnPublish = false)
    {
        return new VisualizationSettings(
            primaryMode,
            allowReaderChoice,
            allowedModes,
            preferredStyle,
            preferredProvider,
            maxImagesPerPage,
            autoGenerateOnPublish);
    }

    /// <summary>
    /// Настройки по умолчанию (визуализация отключена)
    /// </summary>
    public static VisualizationSettings Default()
    {
        return new VisualizationSettings(
            VisualizationMode.None,
            false,
            null,
            null,
            null,
            1,
            false);
    }

    /// <summary>
    /// Пустые настройки (алиас для Default)
    /// </summary>
    public static VisualizationSettings Empty => Default();

    /// <summary>
    /// Настройки для режима "на каждую страницу"
    /// </summary>
    public static VisualizationSettings PerPage(
        string? style = null,
        string? provider = "dalle3",
        bool autoGenerate = false)
    {
        return Create(
            VisualizationMode.PerPage,
            allowReaderChoice: false,
            allowedModes: new[] { VisualizationMode.PerPage },
            preferredStyle: style,
            preferredProvider: provider,
            maxImagesPerPage: 1,
            autoGenerateOnPublish: autoGenerate);
    }

    /// <summary>
    /// Настройки для режима "на каждую главу"
    /// </summary>
    public static VisualizationSettings PerChapter(
        string? style = null,
        string? provider = "dalle3",
        bool autoGenerate = true)
    {
        return Create(
            VisualizationMode.PerChapter,
            allowReaderChoice: true,
            allowedModes: new[] { VisualizationMode.PerChapter },
            preferredStyle: style,
            preferredProvider: provider,
            maxImagesPerPage: 1,
            autoGenerateOnPublish: autoGenerate);
    }

    /// <summary>
    /// Настройки для режима "автор определяет"
    /// </summary>
    public static VisualizationSettings AuthorDefined(
        string? style = null,
        string? provider = "dalle3")
    {
        return Create(
            VisualizationMode.AuthorDefined,
            allowReaderChoice: false,
            allowedModes: null,
            preferredStyle: style,
            preferredProvider: provider,
            maxImagesPerPage: 3,
            autoGenerateOnPublish: false);
    }

    /// <summary>
    /// Настройки для режима "читатель выбирает"
    /// </summary>
    public static VisualizationSettings UserSelected(
        string? style = null,
        string? provider = "dalle3",
        int maxImagesPerPage = 2)
    {
        return Create(
            VisualizationMode.UserSelected,
            allowReaderChoice: true,
            allowedModes: new[]
            {
                VisualizationMode.UserSelected,
                VisualizationMode.PerPage,
                VisualizationMode.PerChapter
            },
            preferredStyle: style,
            preferredProvider: provider,
            maxImagesPerPage: maxImagesPerPage,
            autoGenerateOnPublish: false);
    }

    #endregion

    #region Enable/Disable Methods

    /// <summary>
    /// Включает визуализацию с указанным режимом
    /// </summary>
    public VisualizationSettings Enable(VisualizationMode? mode = null)
    {
        var newMode = mode ?? (PrimaryMode != VisualizationMode.None
            ? PrimaryMode
            : VisualizationMode.PerChapter);

        return new VisualizationSettings(
            newMode,
            AllowReaderChoice,
            _allowedModes,
            PreferredStyle,
            PreferredProvider,
            MaxImagesPerPage,
            AutoGenerateOnPublish);
    }

    /// <summary>
    /// Отключает визуализацию
    /// </summary>
    public VisualizationSettings Disable()
    {
        return new VisualizationSettings(
            VisualizationMode.None,
            false,
            null,
            PreferredStyle,
            PreferredProvider,
            MaxImagesPerPage,
            false);
    }

    #endregion

    #region Modification Methods

    /// <summary>
    /// Возвращает копию с изменённым режимом
    /// </summary>
    public VisualizationSettings WithMode(VisualizationMode mode)
    {
        return new VisualizationSettings(
            mode,
            AllowReaderChoice,
            _allowedModes,
            PreferredStyle,
            PreferredProvider,
            MaxImagesPerPage,
            AutoGenerateOnPublish);
    }

    /// <summary>
    /// Возвращает копию с изменённым стилем
    /// </summary>
    public VisualizationSettings WithStyle(string? style)
    {
        return new VisualizationSettings(
            PrimaryMode,
            AllowReaderChoice,
            _allowedModes,
            style?.Trim(),
            PreferredProvider,
            MaxImagesPerPage,
            AutoGenerateOnPublish);
    }

    /// <summary>
    /// Возвращает копию с изменённым провайдером
    /// </summary>
    public VisualizationSettings WithProvider(string? provider)
    {
        return new VisualizationSettings(
            PrimaryMode,
            AllowReaderChoice,
            _allowedModes,
            PreferredStyle,
            provider?.Trim().ToLowerInvariant(),
            MaxImagesPerPage,
            AutoGenerateOnPublish);
    }

    /// <summary>
    /// Возвращает копию с изменённым флагом выбора читателя
    /// </summary>
    public VisualizationSettings WithReaderChoice(bool allow, IEnumerable<VisualizationMode>? allowedModes = null)
    {
        return new VisualizationSettings(
            PrimaryMode,
            allow,
            allowedModes ?? _allowedModes,
            PreferredStyle,
            PreferredProvider,
            MaxImagesPerPage,
            AutoGenerateOnPublish);
    }

    /// <summary>
    /// Возвращает копию с изменённым макс. количеством изображений
    /// </summary>
    public VisualizationSettings WithMaxImages(int maxImages)
    {
        return new VisualizationSettings(
            PrimaryMode,
            AllowReaderChoice,
            _allowedModes,
            PreferredStyle,
            PreferredProvider,
            maxImages,
            AutoGenerateOnPublish);
    }

    /// <summary>
    /// Возвращает копию с изменённым флагом автогенерации
    /// </summary>
    public VisualizationSettings WithAutoGenerate(bool autoGenerate)
    {
        return new VisualizationSettings(
            PrimaryMode,
            AllowReaderChoice,
            _allowedModes,
            PreferredStyle,
            PreferredProvider,
            MaxImagesPerPage,
            autoGenerate);
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Проверяет, разрешён ли указанный режим
    /// </summary>
    public bool IsModeAllowed(VisualizationMode mode)
    {
        if (!AllowReaderChoice)
            return mode == PrimaryMode;

        return _allowedModes.Count == 0 || _allowedModes.Contains(mode);
    }

    #endregion

    #region ValueObject Implementation

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PrimaryMode;
        yield return AllowReaderChoice;
        yield return PreferredStyle;
        yield return PreferredProvider;
        yield return MaxImagesPerPage;
        yield return AutoGenerateOnPublish;
        yield return IsEnabled;

        foreach (var mode in _allowedModes.OrderBy(m => m.Value))
        {
            yield return mode;
        }
    }

    #endregion
}