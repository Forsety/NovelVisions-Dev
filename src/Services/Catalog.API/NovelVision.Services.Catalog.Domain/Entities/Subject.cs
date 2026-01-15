// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Entities/Subject.cs
using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Entities;

/// <summary>
/// Тематическая категория (жанр, тема, эпоха и т.д.)
/// Поддерживает иерархию через ParentId
/// </summary>
public sealed class Subject : Entity<SubjectId>
{
    // ═══════════════════════════════════════════════════════════════
    // BACKING FIELDS
    // ═══════════════════════════════════════════════════════════════
    private string _name = string.Empty;
    private string? _description;
    private string _slug = string.Empty;  // ДОБАВЛЕНО: backing field для Slug
    private int _bookCount;               // ДОБАВЛЕНО: denormalized counter

    // Для runtime использования (не хранится в БД)
    private readonly HashSet<BookId> _bookIds = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Private parameterless constructor for EF Core
    /// </summary>
    private Subject() : base(default!)
    {
        // EF Core заполнит все поля через reflection
    }

    private Subject(
        SubjectId id,
        string name,
        SubjectType type,
        SubjectId? parentId,
        string? description,
        string? externalMapping) : base(id)
    {
        _name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        _slug = GenerateSlug(name);  // Генерируем slug при создании
        Type = type;
        ParentId = parentId;
        _description = description;
        ExternalMapping = externalMapping;
    }

    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES (с backing fields)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Название категории
    /// </summary>
    public string Name
    {
        get => _name;
        private set
        {
            _name = value;
            _slug = GenerateSlug(value);  // Обновляем slug при изменении name
        }
    }

    /// <summary>
    /// Тип категории (Genre, Topic, Era и т.д.)
    /// </summary>
    public SubjectType Type { get; private set; } = SubjectType.Topic;

    /// <summary>
    /// ID родительской категории (для иерархии)
    /// </summary>
    public SubjectId? ParentId { get; private set; }

    /// <summary>
    /// Описание категории
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value;
    }

    /// <summary>
    /// Маппинг на внешний источник (например, Gutenberg bookshelf name)
    /// </summary>
    public string? ExternalMapping { get; private set; }

    /// <summary>
    /// Slug для URL (хранится в БД)
    /// </summary>
    public string Slug
    {
        get => _slug;
        private set => _slug = value;
    }

    /// <summary>
    /// Количество книг в этой категории (denormalized для производительности)
    /// </summary>
    public int BookCount
    {
        get => _bookCount;
        private set => _bookCount = value;
    }

    /// <summary>
    /// Является ли корневой категорией (без родителя)
    /// </summary>
    public bool IsRoot => ParentId is null;

    // ═══════════════════════════════════════════════════════════════
    // RUNTIME-ONLY PROPERTIES (не хранятся в БД)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// ID книг в этой категории (для runtime использования)
    /// Заполняется через Include() или отдельные запросы
    /// </summary>
    public IReadOnlySet<BookId> BookIds => _bookIds;

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Создание новой категории
    /// </summary>
    public static Subject Create(
        string name,
        SubjectType type,
        SubjectId? parentId = null,
        string? description = null,
        string? externalMapping = null)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.Null(type, nameof(type));

        return new Subject(
            SubjectId.Create(),
            name.Trim(),
            type,
            parentId,
            description?.Trim(),
            externalMapping?.Trim());
    }

    /// <summary>
    /// Создание категории из Gutenberg bookshelf
    /// </summary>
    public static Subject CreateFromGutenberg(string bookshelfName)
    {
        Guard.Against.NullOrWhiteSpace(bookshelfName, nameof(bookshelfName));

        var type = InferTypeFromName(bookshelfName);

        return new Subject(
            SubjectId.Create(),
            bookshelfName.Trim(),
            type,
            null,
            null,
            bookshelfName);
    }

    // ═══════════════════════════════════════════════════════════════
    // MODIFICATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Обновление названия категории
    /// </summary>
    public void UpdateName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Name = name.Trim();  // Setter обновит и _slug
        UpdateTimestamp();
    }

    /// <summary>
    /// Обновление описания
    /// </summary>
    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        UpdateTimestamp();
    }

    /// <summary>
    /// Обновление типа категории
    /// </summary>
    public void UpdateType(SubjectType type)
    {
        Guard.Against.Null(type, nameof(type));
        Type = type;
        UpdateTimestamp();
    }

    /// <summary>
    /// Установка родительской категории
    /// </summary>
    public void SetParent(SubjectId? parentId)
    {
        ParentId = parentId;
        UpdateTimestamp();
    }

    /// <summary>
    /// Обновление внешнего маппинга
    /// </summary>
    public void UpdateExternalMapping(string? mapping)
    {
        ExternalMapping = mapping?.Trim();
        UpdateTimestamp();
    }

    /// <summary>
    /// Инкремент счётчика книг
    /// </summary>
    public void IncrementBookCount()
    {
        _bookCount++;
        UpdateTimestamp();
    }

    /// <summary>
    /// Декремент счётчика книг
    /// </summary>
    public void DecrementBookCount()
    {
        if (_bookCount > 0)
        {
            _bookCount--;
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Установка счётчика книг (для синхронизации)
    /// </summary>
    public void SetBookCount(int count)
    {
        _bookCount = Math.Max(0, count);
        UpdateTimestamp();
    }

    /// <summary>
    /// Добавление книги (runtime only)
    /// </summary>
    public void AddBook(BookId bookId)
    {
        if (_bookIds.Add(bookId))
        {
            IncrementBookCount();
        }
    }

    /// <summary>
    /// Удаление книги (runtime only)
    /// </summary>
    public void RemoveBook(BookId bookId)
    {
        if (_bookIds.Remove(bookId))
        {
            DecrementBookCount();
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Генерация slug из названия
    /// </summary>
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "and")
            .Replace(",", "")
            .Replace(".", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("!", "")
            .Replace("?", "");
    }

    /// <summary>
    /// Определение типа категории по названию
    /// </summary>
    private static SubjectType InferTypeFromName(string name)
    {
        var lowerName = name.ToLowerInvariant();

        // Детектируем аудиторию
        if (lowerName.Contains("children") ||
            lowerName.Contains("young adult") ||
            lowerName.Contains("juvenile"))
        {
            return SubjectType.Audience;
        }

        // Детектируем жанры
        if (lowerName.Contains("fiction") ||
            lowerName.Contains("mystery") ||
            lowerName.Contains("romance") ||
            lowerName.Contains("horror") ||
            lowerName.Contains("fantasy") ||
            lowerName.Contains("science fiction"))
        {
            return SubjectType.Genre;
        }

        // Детектируем эпохи
        if (lowerName.Contains("century") ||
            lowerName.Contains("era") ||
            lowerName.Contains("period") ||
            lowerName.Contains("ancient") ||
            lowerName.Contains("medieval"))
        {
            return SubjectType.Era;
        }

        // Детектируем регионы
        if (lowerName.Contains("england") ||
            lowerName.Contains("france") ||
            lowerName.Contains("america") ||
            lowerName.Contains("europe"))
        {
            return SubjectType.Region;
        }

        // По умолчанию - Topic
        return SubjectType.Topic;
    }
}