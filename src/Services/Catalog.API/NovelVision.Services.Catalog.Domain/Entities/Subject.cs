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
    private string _name = string.Empty;
    private string? _description;
    private readonly HashSet<BookId> _bookIds = new();

    // Private parameterless constructor for EF Core
    private Subject() : base(default!)
    {
    }

    private Subject(
        SubjectId id,
        string name,
        SubjectType type,
        SubjectId? parentId,
        string? description,
        string? externalMapping) : base(id)
    {
        _name = name;
        Type = type;
        ParentId = parentId;
        _description = description;
        ExternalMapping = externalMapping;
    }

    /// <summary>
    /// Название категории
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Тип категории (Genre, Topic, Era и т.д.)
    /// </summary>
    public SubjectType Type { get; private set; } = null!;

    /// <summary>
    /// ID родительской категории (для иерархии)
    /// </summary>
    public SubjectId? ParentId { get; private set; }

    /// <summary>
    /// Описание категории
    /// </summary>
    public string? Description => _description;

    /// <summary>
    /// Маппинг на внешний источник (например, Gutenberg bookshelf name)
    /// </summary>
    public string? ExternalMapping { get; private set; }

    /// <summary>
    /// Slug для URL (автоматически генерируется из имени)
    /// </summary>
    public string Slug => GenerateSlug(_name);

    /// <summary>
    /// Количество книг в этой категории
    /// </summary>
    public int BookCount => _bookIds.Count;

    /// <summary>
    /// ID книг в этой категории
    /// </summary>
    public IReadOnlySet<BookId> BookIds => _bookIds;

    /// <summary>
    /// Является ли корневой категорией (без родителя)
    /// </summary>
    public bool IsRoot => ParentId is null;

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

        // Проверяем поддержку иерархии
        if (parentId is not null && !type.SupportsHierarchy)
        {
            throw new InvalidOperationException(
                $"Subject type '{type.Name}' does not support hierarchy");
        }

        return new Subject(
            SubjectId.Create(),
            name.Trim(),
            type,
            parentId,
            description?.Trim(),
            externalMapping?.Trim());
    }

    /// <summary>
    /// Создание жанра
    /// </summary>
    public static Subject CreateGenre(
        string name,
        SubjectId? parentId = null,
        string? description = null)
    {
        return Create(name, SubjectType.Genre, parentId, description);
    }

    /// <summary>
    /// Создание темы/топика
    /// </summary>
    public static Subject CreateTopic(
        string name,
        SubjectId? parentId = null,
        string? description = null)
    {
        return Create(name, SubjectType.Topic, parentId, description);
    }

    /// <summary>
    /// Создание из Gutenberg bookshelf
    /// </summary>
    public static Subject CreateFromGutenbergBookshelf(
        string bookshelfName,
        SubjectType? typeOverride = null)
    {
        var type = typeOverride ?? InferTypeFromName(bookshelfName);

        return Create(
            name: bookshelfName,
            type: type,
            externalMapping: bookshelfName);
    }

    /// <summary>
    /// Обновление названия
    /// </summary>
    public void UpdateName(string newName)
    {
        Guard.Against.NullOrWhiteSpace(newName, nameof(newName));

        _name = newName.Trim();
        UpdateTimestamp();
    }

    /// <summary>
    /// Обновление описания
    /// </summary>
    public void UpdateDescription(string? description)
    {
        _description = description?.Trim();
        UpdateTimestamp();
    }

    /// <summary>
    /// Установка родительской категории
    /// </summary>
    public void SetParent(SubjectId? parentId)
    {
        if (parentId is not null && !Type.SupportsHierarchy)
        {
            throw new InvalidOperationException(
                $"Subject type '{Type.Name}' does not support hierarchy");
        }

        // Предотвращаем циклические ссылки
        if (parentId == Id)
        {
            throw new InvalidOperationException("Subject cannot be its own parent");
        }

        ParentId = parentId;
        UpdateTimestamp();
    }

    /// <summary>
    /// Добавление книги в категорию
    /// </summary>
    public void AddBook(BookId bookId)
    {
        Guard.Against.Null(bookId, nameof(bookId));

        if (_bookIds.Add(bookId))
        {
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Удаление книги из категории
    /// </summary>
    public void RemoveBook(BookId bookId)
    {
        Guard.Against.Null(bookId, nameof(bookId));

        if (_bookIds.Remove(bookId))
        {
            UpdateTimestamp();
        }
    }

    /// <summary>
    /// Установка маппинга на внешний источник
    /// </summary>
    public void SetExternalMapping(string? mapping)
    {
        ExternalMapping = mapping?.Trim();
        UpdateTimestamp();
    }

    /// <summary>
    /// Генерация slug из названия
    /// </summary>
    private static string GenerateSlug(string name)
    {
        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "and")
            .Replace(",", "");
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