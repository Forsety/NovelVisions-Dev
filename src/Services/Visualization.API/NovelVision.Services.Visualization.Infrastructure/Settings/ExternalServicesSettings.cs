// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Settings/ExternalServicesSettings.cs

namespace NovelVision.Services.Visualization.Infrastructure.Settings;

/// <summary>
/// Настройки внешних сервисов
/// </summary>
public sealed class ExternalServicesSettings
{
    /// <summary>
    /// URL Catalog.API
    /// </summary>
    public string CatalogApiUrl { get; set; } = "https://localhost:7295";

    /// <summary>
    /// URL PromptGen.API
    /// </summary>
    public string PromptGenApiUrl { get; set; } = "http://localhost:8000";

    /// <summary>
    /// Timeout для HTTP запросов (секунды)
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Количество повторных попыток
    /// </summary>
    public int RetryCount { get; set; } = 3;
}