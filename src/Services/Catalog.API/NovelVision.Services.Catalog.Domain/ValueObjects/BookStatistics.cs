// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/ValueObjects/BookStatistics.cs
using System;
using System.Collections.Generic;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Catalog.Domain.ValueObjects;

/// <summary>
/// Статистика книги (Value Object)
/// </summary>
public sealed class BookStatistics : ValueObject
{
    private BookStatistics() { }

    private BookStatistics(
        int downloadCount,
        int viewCount,
        decimal averageRating,
        int reviewCount,
        int favoriteCount)
    {
        DownloadCount = downloadCount;
        ViewCount = viewCount;
        AverageRating = averageRating;
        ReviewCount = reviewCount;
        FavoriteCount = favoriteCount;
        RatingCount = reviewCount; // По умолчанию RatingCount = ReviewCount
    }

    /// <summary>
    /// Количество скачиваний
    /// </summary>
    public int DownloadCount { get; private set; }

    /// <summary>
    /// Количество просмотров
    /// </summary>
    public int ViewCount { get; private set; }

    /// <summary>
    /// Средний рейтинг (0-5)
    /// </summary>
    public decimal AverageRating { get; private set; }

    /// <summary>
    /// Количество отзывов
    /// </summary>
    public int ReviewCount { get; private set; }

    /// <summary>
    /// Количество оценок
    /// </summary>
    public int RatingCount { get; private set; }

    /// <summary>
    /// Количество добавлений в избранное
    /// </summary>
    public int FavoriteCount { get; private set; }

    /// <summary>
    /// Количество завершенных прочтений
    /// </summary>
    public int CompletedReadCount { get; private set; }

    /// <summary>
    /// Количество визуализаций
    /// </summary>
    public int VisualizationCount { get; private set; }

    /// <summary>
    /// Пустая статистика
    /// </summary>
    public static BookStatistics Empty => new(0, 0, 0, 0, 0);

    /// <summary>
    /// Создание статистики
    /// </summary>
    public static BookStatistics Create(
        int downloadCount,
        int viewCount,
        decimal averageRating,
        int reviewCount,
        int favoriteCount)
    {
        return new BookStatistics(
            Math.Max(0, downloadCount),
            Math.Max(0, viewCount),
            Math.Clamp(averageRating, 0, 5),
            Math.Max(0, reviewCount),
            Math.Max(0, favoriteCount));
    }

    /// <summary>
    /// Создание из Gutenberg (только download count)
    /// </summary>
    public static BookStatistics CreateFromGutenberg(int downloadCount)
    {
        return new BookStatistics(Math.Max(0, downloadCount), 0, 0, 0, 0);
    }

    /// <summary>
    /// Обновить количество скачиваний
    /// </summary>
    public BookStatistics WithDownloadCount(int downloadCount)
    {
        return new BookStatistics(
            Math.Max(0, downloadCount),
            ViewCount,
            AverageRating,
            ReviewCount,
            FavoriteCount)
        {
            CompletedReadCount = CompletedReadCount,
            VisualizationCount = VisualizationCount,
            RatingCount = RatingCount
        };
    }

    /// <summary>
    /// Увеличить просмотры
    /// </summary>
    public BookStatistics IncrementViews()
    {
        return new BookStatistics(
            DownloadCount,
            ViewCount + 1,
            AverageRating,
            ReviewCount,
            FavoriteCount)
        {
            CompletedReadCount = CompletedReadCount,
            VisualizationCount = VisualizationCount,
            RatingCount = RatingCount
        };
    }

    /// <summary>
    /// Увеличить избранное
    /// </summary>
    public BookStatistics IncrementFavorites()
    {
        return new BookStatistics(
            DownloadCount,
            ViewCount,
            AverageRating,
            ReviewCount,
            FavoriteCount + 1)
        {
            CompletedReadCount = CompletedReadCount,
            VisualizationCount = VisualizationCount,
            RatingCount = RatingCount
        };
    }

    /// <summary>
    /// Уменьшить избранное
    /// </summary>
    public BookStatistics DecrementFavorites()
    {
        return new BookStatistics(
            DownloadCount,
            ViewCount,
            AverageRating,
            ReviewCount,
            Math.Max(0, FavoriteCount - 1))
        {
            CompletedReadCount = CompletedReadCount,
            VisualizationCount = VisualizationCount,
            RatingCount = RatingCount
        };
    }

    /// <summary>
    /// Добавить рейтинг
    /// </summary>
    public BookStatistics AddRating(decimal rating)
    {
        var newRatingCount = RatingCount + 1;
        var newAverageRating = ((AverageRating * RatingCount) + rating) / newRatingCount;

        return new BookStatistics(
            DownloadCount,
            ViewCount,
            Math.Clamp(newAverageRating, 0, 5),
            ReviewCount,
            FavoriteCount)
        {
            CompletedReadCount = CompletedReadCount,
            VisualizationCount = VisualizationCount,
            RatingCount = newRatingCount
        };
    }

    /// <summary>
    /// Увеличить количество отзывов
    /// </summary>
    public BookStatistics IncrementReviews()
    {
        return new BookStatistics(
            DownloadCount,
            ViewCount,
            AverageRating,
            ReviewCount + 1,
            FavoriteCount)
        {
            CompletedReadCount = CompletedReadCount,
            VisualizationCount = VisualizationCount,
            RatingCount = RatingCount
        };
    }

    /// <summary>
    /// Увеличить количество завершенных прочтений
    /// </summary>
    public BookStatistics IncrementCompletedReads()
    {
        var result = new BookStatistics(
            DownloadCount,
            ViewCount,
            AverageRating,
            ReviewCount,
            FavoriteCount)
        {
            CompletedReadCount = CompletedReadCount + 1,
            VisualizationCount = VisualizationCount,
            RatingCount = RatingCount
        };
        return result;
    }

    /// <summary>
    /// Увеличить количество визуализаций
    /// </summary>
    public BookStatistics IncrementVisualizations()
    {
        var result = new BookStatistics(
            DownloadCount,
            ViewCount,
            AverageRating,
            ReviewCount,
            FavoriteCount)
        {
            CompletedReadCount = CompletedReadCount,
            VisualizationCount = VisualizationCount + 1,
            RatingCount = RatingCount
        };
        return result;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DownloadCount;
        yield return ViewCount;
        yield return AverageRating;
        yield return ReviewCount;
        yield return RatingCount;
        yield return FavoriteCount;
        yield return CompletedReadCount;
        yield return VisualizationCount;
    }
}