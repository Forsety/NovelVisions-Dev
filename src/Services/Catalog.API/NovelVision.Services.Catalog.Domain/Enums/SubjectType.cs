// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Enums/SubjectType.cs
using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.Enums;

/// <summary>
/// Тип тематики/категории
/// </summary>
public sealed class SubjectType : SmartEnum<SubjectType>
{
    /// <summary>
    /// Жанр (Fiction, Non-Fiction, etc.)
    /// </summary>
    public static readonly SubjectType Genre = new(nameof(Genre), 1, "Genre", "Literary genre classification", true);

    /// <summary>
    /// Тема (Love, War, Adventure, etc.)
    /// </summary>
    public static readonly SubjectType Topic = new(nameof(Topic), 2, "Topic", "Subject matter or theme", true);

    /// <summary>
    /// Период времени (Victorian Era, Medieval, etc.)
    /// </summary>
    public static readonly SubjectType TimePeriod = new(nameof(TimePeriod), 3, "Time Period", "Historical time period", false);

    /// <summary>
    /// Географическое место (England, Space, etc.)
    /// </summary>
    public static readonly SubjectType Place = new(nameof(Place), 4, "Place", "Geographic location or setting", true);

    /// <summary>
    /// Персона (биографии и т.п.)
    /// </summary>
    public static readonly SubjectType Person = new(nameof(Person), 5, "Person", "Historical or fictional person", false);

    /// <summary>
    /// Целевая аудитория (Children, Young Adult, etc.)
    /// </summary>
    public static readonly SubjectType Audience = new(nameof(Audience), 6, "Audience", "Target audience or age group", false);

    /// <summary>
    /// Форма/формат (Poetry, Drama, etc.)
    /// </summary>
    public static readonly SubjectType Form = new(nameof(Form), 7, "Form", "Literary form or format", false);

    /// <summary>
    /// Импортированная категория Gutenberg
    /// </summary>
    public static readonly SubjectType GutenbergSubject = new(nameof(GutenbergSubject), 8, "Gutenberg Subject", "Subject imported from Project Gutenberg", false);

    /// <summary>
    /// Пользовательский тег
    /// </summary>
    public static readonly SubjectType UserTag = new(nameof(UserTag), 9, "User Tag", "User-defined tag", false);

    /// <summary>
    /// Эпоха (Ancient, Medieval, Modern, etc.)
    /// </summary>
    public static readonly SubjectType Era = new(nameof(Era), 10, "Era", "Historical era or age", false);

    /// <summary>
    /// Регион (Europe, Asia, Americas, etc.)
    /// </summary>
    public static readonly SubjectType Region = new(nameof(Region), 11, "Region", "Geographic region", true);

    /// <summary>
    /// Полка Gutenberg (Bookshelf)
    /// </summary>
    public static readonly SubjectType Bookshelf = new(nameof(Bookshelf), 12, "Bookshelf", "Project Gutenberg bookshelf category", false);

    private SubjectType(string name, int value, string displayName, string description, bool supportsHierarchy)
        : base(name, value)
    {
        DisplayName = displayName;
        Description = description;
        SupportsHierarchy = supportsHierarchy;
    }

    /// <summary>
    /// Отображаемое название
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Описание типа категории
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Поддерживает ли тип иерархию (родитель-потомок)
    /// </summary>
    public bool SupportsHierarchy { get; }
}