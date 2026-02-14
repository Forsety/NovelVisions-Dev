// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Settings/AzureStorageSettings.cs

namespace NovelVision.Services.Visualization.Infrastructure.Settings;

/// <summary>
/// Настройки Azure Blob Storage
/// </summary>
public sealed class AzureStorageSettings
{
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Имя контейнера для изображений
    /// </summary>
    public string ImagesContainer { get; set; } = "visualization-images";

    /// <summary>
    /// Имя контейнера для миниатюр
    /// </summary>
    public string ThumbnailsContainer { get; set; } = "visualization-thumbnails";

    /// <summary>
    /// Базовый URL для CDN (если используется)
    /// </summary>
    public string? CdnBaseUrl { get; set; }

    /// <summary>
    /// Время жизни SAS token в часах
    /// </summary>
    public int SasTokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// Максимальный размер файла в MB
    /// </summary>
    public int MaxFileSizeMb { get; set; } = 10;
}