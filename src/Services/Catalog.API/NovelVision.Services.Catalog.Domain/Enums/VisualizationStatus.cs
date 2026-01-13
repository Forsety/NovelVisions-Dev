// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Enums/VisualizationStatus.cs
// Статус визуализации (SmartEnum)
using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.Enums;

/// <summary>
/// Статус визуализации (SmartEnum)
/// </summary>
public sealed class VisualizationStatus : SmartEnum<VisualizationStatus>
{
    #region Values

    /// <summary>
    /// Нет визуализации
    /// </summary>
    public static readonly VisualizationStatus None = new(nameof(None), 0);

    /// <summary>
    /// Ожидает генерации
    /// </summary>
    public static readonly VisualizationStatus Pending = new(nameof(Pending), 1);

    /// <summary>
    /// В процессе генерации
    /// </summary>
    public static readonly VisualizationStatus InProgress = new(nameof(InProgress), 2);

    /// <summary>
    /// Обработка (алиас для InProgress)
    /// </summary>
    public static readonly VisualizationStatus Processing = new(nameof(Processing), 2);

    /// <summary>
    /// Успешно завершено
    /// </summary>
    public static readonly VisualizationStatus Completed = new(nameof(Completed), 3);

    /// <summary>
    /// Успех (алиас для Completed)
    /// </summary>
    public static readonly VisualizationStatus Success = new(nameof(Success), 3);

    /// <summary>
    /// Ошибка
    /// </summary>
    public static readonly VisualizationStatus Failed = new(nameof(Failed), 4);

    /// <summary>
    /// Ошибка (алиас для Failed)
    /// </summary>
    public static readonly VisualizationStatus Error = new(nameof(Error), 4);

    #endregion

    #region Constructor

    private VisualizationStatus(string name, int value) : base(name, value) { }

    #endregion

    #region Properties

    /// <summary>
    /// Является ли статус финальным
    /// </summary>
    public bool IsFinal => this == Completed || this == Success || this == Failed || this == Error;

    /// <summary>
    /// В процессе ли выполнение
    /// </summary>
    public bool IsActive => this == Pending || this == InProgress || this == Processing;

    /// <summary>
    /// Успешно ли завершено
    /// </summary>
    public bool IsSuccess => this == Completed || this == Success;

    /// <summary>
    /// Завершилось ли с ошибкой
    /// </summary>
    public bool IsError => this == Failed || this == Error;

    #endregion

    #region Static Methods

    /// <summary>
    /// Парсит строку в статус
    /// </summary>
    public static VisualizationStatus FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return None;

        return TryFromName(value, ignoreCase: true, out var status)
            ? status
            : None;
    }

    #endregion
}