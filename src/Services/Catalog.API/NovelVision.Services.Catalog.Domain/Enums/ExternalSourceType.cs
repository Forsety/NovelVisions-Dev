// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Enums/ExternalSourceType.cs
// ИСПРАВЛЕНИЕ: Добавлен метод ToBookSource()
using Ardalis.SmartEnum;

namespace NovelVision.Services.Catalog.Domain.Enums;

/// <summary>
/// Тип внешнего источника книг
/// </summary>
public sealed class ExternalSourceType : SmartEnum<ExternalSourceType>
{
    /// <summary>
    /// Ручной ввод (не из внешнего источника)
    /// </summary>
    public static readonly ExternalSourceType Manual = new(nameof(Manual), 0, "Manual Entry");

    /// <summary>
    /// Project Gutenberg
    /// </summary>
    public static readonly ExternalSourceType Gutenberg = new(nameof(Gutenberg), 1, "Project Gutenberg");

    /// <summary>
    /// Open Library
    /// </summary>
    public static readonly ExternalSourceType OpenLibrary = new(nameof(OpenLibrary), 2, "Open Library");

    /// <summary>
    /// Google Books
    /// </summary>
    public static readonly ExternalSourceType GoogleBooks = new(nameof(GoogleBooks), 3, "Google Books");

    /// <summary>
    /// Internet Archive
    /// </summary>
    public static readonly ExternalSourceType InternetArchive = new(nameof(InternetArchive), 4, "Internet Archive");

    /// <summary>
    /// Standard Ebooks
    /// </summary>
    public static readonly ExternalSourceType StandardEbooks = new(nameof(StandardEbooks), 5, "Standard Ebooks");

    private ExternalSourceType(string name, int value, string displayName)
        : base(name, value)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Отображаемое название
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Является ли источником общественного достояния
    /// </summary>
    public bool IsPublicDomainSource => this == Gutenberg ||
                                         this == InternetArchive ||
                                         this == StandardEbooks;

    /// <summary>
    /// Базовый URL источника
    /// </summary>
    public string? BaseUrl => Name switch
    {
        nameof(Gutenberg) => "https://www.gutenberg.org",
        nameof(OpenLibrary) => "https://openlibrary.org",
        nameof(GoogleBooks) => "https://books.google.com",
        nameof(InternetArchive) => "https://archive.org",
        nameof(StandardEbooks) => "https://standardebooks.org",
        _ => null
    };

    /// <summary>
    /// Конвертирует в BookSource
    /// </summary>
    public BookSource ToBookSource()
    {
        return Name switch
        {
            nameof(Manual) => BookSource.UserCreated,
            nameof(Gutenberg) => BookSource.Gutenberg,
            nameof(OpenLibrary) => BookSource.OpenLibrary,
            nameof(GoogleBooks) => BookSource.External,
            nameof(InternetArchive) => BookSource.External,
            nameof(StandardEbooks) => BookSource.External,
            _ => BookSource.Unknown
        };
    }
}

/// <summary>
/// Extension methods для ExternalSourceType
/// </summary>
public static class ExternalSourceTypeExtensions
{
    /// <summary>
    /// Конвертирует ExternalSourceType в BookSource
    /// </summary>
    public static BookSource ToBookSource(this ExternalSourceType sourceType)
    {
        if (sourceType is null)
            return BookSource.Unknown;

        return sourceType.Name switch
        {
            nameof(ExternalSourceType.Manual) => BookSource.UserCreated,
            nameof(ExternalSourceType.Gutenberg) => BookSource.Gutenberg,
            nameof(ExternalSourceType.OpenLibrary) => BookSource.OpenLibrary,
            nameof(ExternalSourceType.GoogleBooks) => BookSource.External,
            nameof(ExternalSourceType.InternetArchive) => BookSource.External,
            nameof(ExternalSourceType.StandardEbooks) => BookSource.External,
            _ => BookSource.Unknown
        };
    }
}