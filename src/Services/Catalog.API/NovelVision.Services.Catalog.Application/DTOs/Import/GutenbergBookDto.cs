// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/Import/GutenbergBookDto.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace NovelVision.Services.Catalog.Application.DTOs.Import;

/// <summary>
/// DTO для данных книги из Gutendex API
/// </summary>
public record GutenbergBookDto
{
    /// <summary>
    /// ID книги в Gutenberg
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Название книги
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Список авторов
    /// </summary>
    public List<GutenbergAuthorDto> Authors { get; init; } = new();

    /// <summary>
    /// Список переводчиков
    /// </summary>
    public List<GutenbergAuthorDto> Translators { get; init; } = new();

    /// <summary>
    /// Темы/категории книги
    /// </summary>
    public List<string> Subjects { get; init; } = new();

    /// <summary>
    /// Полки (коллекции) в Gutenberg
    /// </summary>
    public List<string> Bookshelves { get; init; } = new();

    /// <summary>
    /// Языки книги
    /// </summary>
    public List<string> Languages { get; init; } = new();

    /// <summary>
    /// Защищена ли книга авторским правом
    /// </summary>
    public bool? Copyright { get; init; }

    /// <summary>
    /// Тип медиа
    /// </summary>
    public string MediaType { get; init; } = string.Empty;

    /// <summary>
    /// Доступные форматы книги
    /// </summary>
    public GutenbergFormatsDto Formats { get; init; } = new();

    /// <summary>
    /// Количество скачиваний
    /// </summary>
    public int DownloadCount { get; init; }

    #region Computed Properties

    /// <summary>
    /// Основной автор книги (первый в списке)
    /// </summary>
    public GutenbergAuthorDto? PrimaryAuthor => Authors.FirstOrDefault();

    /// <summary>
    /// Основной язык книги (первый в списке)
    /// </summary>
    public string PrimaryLanguage => Languages.FirstOrDefault() ?? "en";

    /// <summary>
    /// URL обложки книги (из форматов)
    /// </summary>
    public string? CoverImageUrl => Formats?.ImageJpeg;

    /// <summary>
    /// URL текста книги (plain text с UTF-8)
    /// </summary>
    public string? TextUrl => Formats?.TextPlainUtf8 ?? Formats?.TextPlain;

    /// <summary>
    /// URL HTML версии книги
    /// </summary>
    public string? HtmlUrl => Formats?.TextHtml;

    /// <summary>
    /// Есть ли у книги обложка
    /// </summary>
    public bool HasCover => !string.IsNullOrEmpty(CoverImageUrl);

    /// <summary>
    /// Есть ли текстовая версия
    /// </summary>
    public bool HasText => !string.IsNullOrEmpty(TextUrl);

    /// <summary>
    /// Является ли книга общественным достоянием
    /// </summary>
    public bool IsPublicDomain => Copyright == false;

    /// <summary>
    /// URL страницы книги на Gutenberg
    /// </summary>
    public string GutenbergUrl => $"https://www.gutenberg.org/ebooks/{Id}";

    #endregion
}

/// <summary>
/// DTO для автора из Gutendex
/// </summary>
public record GutenbergAuthorDto
{
    /// <summary>
    /// Полное имя автора
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Год рождения
    /// </summary>
    public int? BirthYear { get; init; }

    /// <summary>
    /// Год смерти
    /// </summary>
    public int? DeathYear { get; init; }

    /// <summary>
    /// Отображаемое имя (форматированное)
    /// </summary>
    public string DisplayName => FormatDisplayName();

    /// <summary>
    /// Годы жизни в формате "YYYY-YYYY"
    /// </summary>
    public string? LifeYears => FormatLifeYears();

    private string FormatDisplayName()
    {
        // Gutenberg часто использует формат "Фамилия, Имя"
        // Преобразуем в "Имя Фамилия"
        if (string.IsNullOrWhiteSpace(Name))
            return "Unknown Author";

        var parts = Name.Split(',', 2);
        if (parts.Length == 2)
        {
            return $"{parts[1].Trim()} {parts[0].Trim()}";
        }
        return Name;
    }

    private string? FormatLifeYears()
    {
        if (!BirthYear.HasValue && !DeathYear.HasValue)
            return null;

        var birth = BirthYear?.ToString() ?? "?";
        var death = DeathYear?.ToString() ?? "";

        return DeathYear.HasValue ? $"{birth}-{death}" : $"{birth}-";
    }
}

/// <summary>
/// DTO для форматов из Gutendex
/// </summary>
public record GutenbergFormatsDto
{
    /// <summary>
    /// Plain text версия
    /// </summary>
    public string? TextPlain { get; init; }

    /// <summary>
    /// Plain text версия в UTF-8
    /// </summary>
    public string? TextPlainUtf8 { get; init; }

    /// <summary>
    /// HTML версия
    /// </summary>
    public string? TextHtml { get; init; }

    /// <summary>
    /// EPUB версия
    /// </summary>
    public string? ApplicationEpub { get; init; }

    /// <summary>
    /// PDF версия
    /// </summary>
    public string? ApplicationPdf { get; init; }

    /// <summary>
    /// Обложка в JPEG
    /// </summary>
    public string? ImageJpeg { get; init; }
}

/// <summary>
/// DTO для ответа поиска Gutendex
/// </summary>
public record GutenbergSearchResultDto
{
    /// <summary>
    /// Общее количество найденных книг
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// URL следующей страницы
    /// </summary>
    public string? Next { get; init; }

    /// <summary>
    /// URL предыдущей страницы
    /// </summary>
    public string? Previous { get; init; }

    /// <summary>
    /// Результаты на текущей странице
    /// </summary>
    public List<GutenbergBookDto> Results { get; init; } = new();

    /// <summary>
    /// Есть ли ещё страницы
    /// </summary>
    public bool HasMore => !string.IsNullOrEmpty(Next);
}

/// <summary>
/// DTO для критериев поиска в Gutendex
/// </summary>
public record GutenbergSearchCriteriaDto
{
    /// <summary>
    /// Поисковый запрос
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Фильтр по языкам
    /// </summary>
    public List<string>? Languages { get; init; }

    /// <summary>
    /// Фильтр по теме
    /// </summary>
    public string? Topic { get; init; }

    /// <summary>
    /// Год рождения автора от
    /// </summary>
    public int? AuthorYearStart { get; init; }

    /// <summary>
    /// Год рождения автора до
    /// </summary>
    public int? AuthorYearEnd { get; init; }

    /// <summary>
    /// Фильтр по авторским правам
    /// </summary>
    public bool? Copyright { get; init; }

    /// <summary>
    /// Список ID книг для загрузки
    /// </summary>
    public List<int>? Ids { get; init; }

    /// <summary>
    /// Сортировка (popular, ascending, descending)
    /// </summary>
    public string Sort { get; init; } = "popular";

    /// <summary>
    /// Номер страницы
    /// </summary>
    public int Page { get; init; } = 1;
}