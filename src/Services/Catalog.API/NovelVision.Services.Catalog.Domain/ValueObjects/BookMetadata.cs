// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/BookMetadata.cs
using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Метаданные книги (Value Object)
/// </summary>
public sealed class BookMetadata : ValueObject
{
    private BookMetadata() { }

    private BookMetadata(
        string title,
        string? subtitle,
        string? description,
        string language,
        int pageCount,
        int wordCount,
        string? originalTitle)
    {
        Title = title;
        Subtitle = subtitle;
        Description = description;
        Language = language;
        PageCount = pageCount;
        WordCount = wordCount;
        OriginalTitle = originalTitle;
    }

    /// <summary>
    /// Название книги
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Подзаголовок
    /// </summary>
    public string? Subtitle { get; private set; }

    /// <summary>
    /// Описание книги
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Язык книги (ISO 639-1)
    /// </summary>
    public string Language { get; private set; } = "en";

    /// <summary>
    /// Количество страниц
    /// </summary>
    public int PageCount { get; private set; }

    /// <summary>
    /// Количество слов
    /// </summary>
    public int WordCount { get; private set; }

    /// <summary>
    /// Оригинальное название (для переводов)
    /// </summary>
    public string? OriginalTitle { get; private set; }

    /// <summary>
    /// Пустые метаданные
    /// </summary>
    public static BookMetadata Empty => new(
        title: "Untitled",
        subtitle: null,
        description: null,
        language: "en",
        pageCount: 0,
        wordCount: 0,
        originalTitle: null);

    /// <summary>
    /// Создание метаданных книги
    /// </summary>
    public static BookMetadata Create(
        string title,
        string? description = null,
        string language = "en",
        int pageCount = 0,
        int wordCount = 0)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));

        return new BookMetadata(
            title.Trim(),
            null,
            description?.Trim(),
            language,
            Math.Max(0, pageCount),
            Math.Max(0, wordCount),
            null);
    }

    /// <summary>
    /// Создание метаданных с подзаголовком
    /// </summary>
    public static BookMetadata Create(
        string title,
        string? subtitle,
        string? description,
        string language,
        int pageCount = 0)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));

        return new BookMetadata(
            title.Trim(),
            subtitle?.Trim(),
            description?.Trim(),
            language,
            Math.Max(0, pageCount),
            0,
            null);
    }

    /// <summary>
    /// Полное создание метаданных
    /// </summary>
    public static BookMetadata CreateFull(
        string title,
        string? subtitle,
        string? description,
        string language,
        int pageCount,
        int wordCount,
        string? originalTitle = null)
    {
        Guard.Against.NullOrWhiteSpace(title, nameof(title));

        return new BookMetadata(
            title.Trim(),
            subtitle?.Trim(),
            description?.Trim(),
            language,
            Math.Max(0, pageCount),
            Math.Max(0, wordCount),
            originalTitle?.Trim());
    }

    /// <summary>
    /// Обновить описание
    /// </summary>
    public BookMetadata WithDescription(string? description)
    {
        return new BookMetadata(
            Title,
            Subtitle,
            description?.Trim(),
            Language,
            PageCount,
            WordCount,
            OriginalTitle);
    }

    /// <summary>
    /// Обновить количество страниц
    /// </summary>
    public BookMetadata WithPageCount(int pageCount)
    {
        return new BookMetadata(
            Title,
            Subtitle,
            Description,
            Language,
            Math.Max(0, pageCount),
            WordCount,
            OriginalTitle);
    }

    /// <summary>
    /// Обновить количество слов
    /// </summary>
    public BookMetadata WithWordCount(int wordCount)
    {
        return new BookMetadata(
            Title,
            Subtitle,
            Description,
            Language,
            PageCount,
            Math.Max(0, wordCount),
            OriginalTitle);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Title;
        yield return Subtitle;
        yield return Description;
        yield return Language;
        yield return PageCount;
        yield return WordCount;
        yield return OriginalTitle;
    }
}