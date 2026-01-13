using Ardalis.SmartEnum;

namespace NovelVision.Services.Visualization.Domain.Enums;

/// <summary>
/// Статус задания визуализации
/// </summary>
public sealed class VisualizationJobStatus : SmartEnum<VisualizationJobStatus>
{
    /// <summary>
    /// Задание создано, ожидает обработки
    /// </summary>
    public static readonly VisualizationJobStatus Pending = new(nameof(Pending), 1, "Ожидание");

    /// <summary>
    /// Задание в очереди на обработку
    /// </summary>
    public static readonly VisualizationJobStatus Queued = new(nameof(Queued), 2, "В очереди");

    /// <summary>
    /// Генерация промпта через PromptGen.API
    /// </summary>
    public static readonly VisualizationJobStatus GeneratingPrompt = new(nameof(GeneratingPrompt), 3, "Генерация промпта");

    /// <summary>
    /// Отправлено на AI модель, ожидание результата
    /// </summary>
    public static readonly VisualizationJobStatus Processing = new(nameof(Processing), 4, "Обработка AI");

    /// <summary>
    /// Загрузка и сохранение результата
    /// </summary>
    public static readonly VisualizationJobStatus Uploading = new(nameof(Uploading), 5, "Загрузка");

    /// <summary>
    /// Успешно завершено
    /// </summary>
    public static readonly VisualizationJobStatus Completed = new(nameof(Completed), 6, "Завершено");

    /// <summary>
    /// Ошибка при обработке
    /// </summary>
    public static readonly VisualizationJobStatus Failed = new(nameof(Failed), 7, "Ошибка");

    /// <summary>
    /// Отменено пользователем или системой
    /// </summary>
    public static readonly VisualizationJobStatus Cancelled = new(nameof(Cancelled), 8, "Отменено");

    private VisualizationJobStatus(string name, int value, string displayName) 
        : base(name, value)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    /// <summary>
    /// Можно ли отменить задание в этом статусе
    /// </summary>
    public bool CanCancel => this == Pending || this == Queued;

    /// <summary>
    /// Можно ли повторить задание в этом статусе
    /// </summary>
    public bool CanRetry => this == Failed || this == Cancelled;

    /// <summary>
    /// Задание в финальном состоянии
    /// </summary>
    public bool IsFinal => this == Completed || this == Failed || this == Cancelled;

    /// <summary>
    /// Задание в активном состоянии обработки
    /// </summary>
    public bool IsProcessing => this == GeneratingPrompt || this == Processing || this == Uploading;
}
