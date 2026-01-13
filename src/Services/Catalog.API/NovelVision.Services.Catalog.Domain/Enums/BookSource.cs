// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Enums/BookSource.cs
using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.Enums;

/// <summary>
/// Источник/происхождение книги
/// </summary>
public sealed class BookSource : SmartEnum<BookSource>
{
    /// <summary>
    /// Оригинальная работа автора платформы
    /// </summary>
    public static readonly BookSource Original = new(nameof(Original), 1, "Original Work");

    /// <summary>
    /// Создано пользователем (алиас для Original)
    /// </summary>
    public static readonly BookSource UserCreated = new(nameof(UserCreated), 1, "User Created");

    /// <summary>
    /// Импортировано из Project Gutenberg
    /// </summary>
    public static readonly BookSource Gutenberg = new(nameof(Gutenberg), 2, "Project Gutenberg");

    /// <summary>
    /// Импортировано из Open Library
    /// </summary>
    public static readonly BookSource OpenLibrary = new(nameof(OpenLibrary), 3, "Open Library");

    /// <summary>
    /// Загружено пользователем
    /// </summary>
    public static readonly BookSource UserUpload = new(nameof(UserUpload), 4, "User Upload");

    /// <summary>
    /// Партнёрский контент
    /// </summary>
    public static readonly BookSource Partner = new(nameof(Partner), 5, "Partner Content");

    /// <summary>
    /// Внешний источник (общий)
    /// </summary>
    public static readonly BookSource External = new(nameof(External), 6, "External Source");

    /// <summary>
    /// Неизвестный источник / по умолчанию
    /// </summary>
    public static readonly BookSource Unknown = new(nameof(Unknown), 0, "Unknown");

    private BookSource(string name, int value, string displayName)
        : base(name, value)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Отображаемое название
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Является ли общественным достоянием
    /// </summary>
    public bool IsPublicDomain => this == Gutenberg || this == OpenLibrary;

    /// <summary>
    /// Требуется ли модерация
    /// </summary>
    public bool RequiresModeration => this == UserUpload;

    /// <summary>
    /// Является ли внешним источником
    /// </summary>
    public bool IsExternalSource => this == Gutenberg || this == OpenLibrary || this == External || this == Partner;

    /// <summary>
    /// Является ли пользовательским контентом
    /// </summary>
    public bool IsUserContent => this == Original || this == UserCreated || this == UserUpload;
}