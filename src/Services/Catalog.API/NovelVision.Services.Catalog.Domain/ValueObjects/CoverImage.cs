// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/CoverImage.cs
using System;
using System.Collections.Generic;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Обложка книги (Value Object)
/// </summary>
public sealed class CoverImage : ValueObject
{
    private CoverImage() { }

    private CoverImage(string url, string? thumbnailUrl, string? altText, string? source)
    {
        Url = url;
        ThumbnailUrl = thumbnailUrl;
        AltText = altText;
        Source = source;
    }

    /// <summary>
    /// URL полноразмерного изображения
    /// </summary>
    public string Url { get; private set; } = string.Empty;

    /// <summary>
    /// URL миниатюры
    /// </summary>
    public string? ThumbnailUrl { get; private set; }

    /// <summary>
    /// Alt текст для изображения
    /// </summary>
    public string? AltText { get; private set; }

    /// <summary>
    /// Источник изображения (Gutenberg, OpenLibrary, Upload и т.д.)
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    /// Есть ли обложка
    /// </summary>
    public bool HasImage => !string.IsNullOrEmpty(Url);

    /// <summary>
    /// Пустая обложка
    /// </summary>
    public static CoverImage Empty => new(string.Empty, null, null, null);

    /// <summary>
    /// Создание обложки
    /// </summary>
    public static CoverImage Create(string url, string? source = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Empty;

        return new CoverImage(url.Trim(), null, null, source);
    }

    /// <summary>
    /// Создание обложки с миниатюрой
    /// </summary>
    public static CoverImage Create(string url, string? thumbnailUrl, string? altText = null, string? source = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Empty;

        return new CoverImage(
            url.Trim(),
            thumbnailUrl?.Trim(),
            altText?.Trim(),
            source);
    }

    /// <summary>
    /// Создание из Gutenberg
    /// </summary>
    public static CoverImage FromGutenberg(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Empty;

        return new CoverImage(imageUrl, imageUrl, null, "Gutenberg");
    }

    /// <summary>
    /// Создание из OpenLibrary
    /// </summary>
    public static CoverImage FromOpenLibrary(string coverId, string size = "L")
    {
        if (string.IsNullOrWhiteSpace(coverId))
            return Empty;

        var url = $"https://covers.openlibrary.org/b/id/{coverId}-{size}.jpg";
        var thumbnailUrl = $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg";

        return new CoverImage(url, thumbnailUrl, null, "OpenLibrary");
    }

    /// <summary>
    /// Обновить URL
    /// </summary>
    public CoverImage WithUrl(string url)
    {
        return new CoverImage(url, ThumbnailUrl, AltText, Source);
    }

    /// <summary>
    /// Обновить миниатюру
    /// </summary>
    public CoverImage WithThumbnail(string? thumbnailUrl)
    {
        return new CoverImage(Url, thumbnailUrl, AltText, Source);
    }

    /// <summary>
    /// Обновить alt текст
    /// </summary>
    public CoverImage WithAltText(string? altText)
    {
        return new CoverImage(Url, ThumbnailUrl, altText, Source);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Url;
        yield return ThumbnailUrl;
        yield return AltText;
        yield return Source;
    }
}