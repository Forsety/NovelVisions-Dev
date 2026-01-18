// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Settings/RedisSettings.cs

namespace NovelVision.Services.Visualization.Infrastructure.Settings;

/// <summary>
/// Настройки Redis
/// </summary>
public sealed class RedisSettings
{
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Префикс для ключей
    /// </summary>
    public string InstanceName { get; set; } = "NovelVision.Visualization:";

    /// <summary>
    /// Время жизни кэша заданий (минуты)
    /// </summary>
    public int JobCacheMinutes { get; set; } = 60;

    /// <summary>
    /// Время жизни кэша очереди (секунды)
    /// </summary>
    public int QueueCacheSeconds { get; set; } = 30;

    /// <summary>
    /// Имя очереди заданий
    /// </summary>
    public string JobQueueName { get; set; } = "visualization:jobs:queue";

    /// <summary>
    /// Имя sorted set для приоритетной очереди
    /// </summary>
    public string PriorityQueueName { get; set; } = "visualization:jobs:priority";
}