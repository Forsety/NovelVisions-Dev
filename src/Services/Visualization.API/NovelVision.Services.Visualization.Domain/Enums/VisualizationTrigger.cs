using Ardalis.SmartEnum;

namespace NovelVision.Services.Visualization.Domain.Enums;

/// <summary>
/// Тип триггера визуализации - как была инициирована визуализация
/// </summary>
public sealed class VisualizationTrigger : SmartEnum<VisualizationTrigger>
{
    /// <summary>
    /// Кнопка "Визуализируй" на странице/главе
    /// </summary>
    public static readonly VisualizationTrigger Button = new(nameof(Button), 1, "Кнопка визуализации");

    /// <summary>
    /// Выделение текста читателем
    /// </summary>
    public static readonly VisualizationTrigger TextSelection = new(nameof(TextSelection), 2, "Выделение текста");

    /// <summary>
    /// Автоматическая веб-новелла (при публикации книги)
    /// </summary>
    public static readonly VisualizationTrigger AutoNovel = new(nameof(AutoNovel), 3, "Авто веб-новелла");

    /// <summary>
    /// Авторские места визуализации
    /// </summary>
    public static readonly VisualizationTrigger AuthorDefined = new(nameof(AuthorDefined), 4, "Авторские места");

    /// <summary>
    /// Визуализация по главе целиком
    /// </summary>
    public static readonly VisualizationTrigger PerChapter = new(nameof(PerChapter), 5, "По главе");

    /// <summary>
    /// Визуализация каждой страницы
    /// </summary>
    public static readonly VisualizationTrigger PerPage = new(nameof(PerPage), 6, "По странице");

    /// <summary>
    /// Повторная генерация (регенерация)
    /// </summary>
    public static readonly VisualizationTrigger Regeneration = new(nameof(Regeneration), 7, "Регенерация");

    private VisualizationTrigger(string name, int value, string displayName)
        : base(name, value)
    {
        DisplayName = displayName;
    }

    public string DisplayName { get; }

    /// <summary>
    /// Требуется ли пользовательский ввод (выделение текста)
    /// </summary>
    public bool RequiresUserInput => this == TextSelection;

    /// <summary>
    /// Автоматический триггер (без действий пользователя)
    /// </summary>
    public bool IsAutomatic => this == AutoNovel || this == PerChapter || this == PerPage;

    /// <summary>
    /// Определён автором книги
    /// </summary>
    public bool IsAuthorControlled => this == AuthorDefined;
}
