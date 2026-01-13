// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/AuthorDto.cs
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO автора с полной информацией
/// </summary>
public record AuthorDto
{
    /// <summary>
    /// ID автора
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Отображаемое имя
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Email автора
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Биография
    /// </summary>
    public string? Biography { get; init; }

    /// <summary>
    /// URL аватара
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Подтвержден ли автор
    /// </summary>
    public bool IsVerified { get; init; }

    /// <summary>
    /// Дата подтверждения
    /// </summary>
    public DateTime? VerifiedAt { get; init; }

    /// <summary>
    /// Количество книг
    /// </summary>
    public int BookCount { get; init; }

    /// <summary>
    /// ID книг автора
    /// </summary>
    public List<Guid> BookIds { get; init; } = new();

    /// <summary>
    /// Социальные ссылки (platform -> url)
    /// </summary>
    public Dictionary<string, string> SocialLinks { get; init; } = new();

    /// <summary>
    /// Год рождения
    /// </summary>
    public int? BirthYear { get; init; }

    /// <summary>
    /// Год смерти
    /// </summary>
    public int? DeathYear { get; init; }

    /// <summary>
    /// Период жизни ("1828-1910")
    /// </summary>
    public string? LifeSpan { get; init; }

    /// <summary>
    /// Национальность/страна
    /// </summary>
    public string? Nationality { get; init; }

    /// <summary>
    /// Жив ли автор
    /// </summary>
    public bool? IsAlive { get; init; }

    /// <summary>
    /// Исторический ли автор (из Gutenberg)
    /// </summary>
    public bool IsHistorical { get; init; }

    /// <summary>
    /// Внешние идентификаторы
    /// </summary>
    public ExternalAuthorIdentifiersDto? ExternalIds { get; init; }

    /// <summary>
    /// Импортирован из внешнего источника
    /// </summary>
    public bool IsFromExternalSource { get; init; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата обновления
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO автора для списков (краткая версия)
/// </summary>
public record AuthorListDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public int BookCount { get; init; }
    public string? LifeSpan { get; init; }
    public string? Nationality { get; init; }
    public bool IsHistorical { get; init; }
    public string? AvatarUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO автора с детальной информацией и книгами
/// </summary>
public record AuthorDetailDto : AuthorDto
{
    /// <summary>
    /// Книги автора
    /// </summary>
    public List<BookListDto> Books { get; init; } = new();
}