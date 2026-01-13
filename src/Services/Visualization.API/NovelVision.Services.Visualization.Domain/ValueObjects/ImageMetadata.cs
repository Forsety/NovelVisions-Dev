using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Visualization.Domain.Enums;

namespace NovelVision.Services.Visualization.Domain.ValueObjects;

/// <summary>
/// Метаданные сгенерированного изображения
/// </summary>
public sealed class ImageMetadata : ValueObject
{
    private ImageMetadata() { }

    private ImageMetadata(
        string url,
        string? thumbnailUrl,
        int width,
        int height,
        long fileSizeBytes,
        ImageFormat format,
        string? blobPath)
    {
        Url = url;
        ThumbnailUrl = thumbnailUrl;
        Width = width;
        Height = height;
        FileSizeBytes = fileSizeBytes;
        Format = format;
        BlobPath = blobPath;
    }

    /// <summary>
    /// URL для доступа к изображению
    /// </summary>
    public string Url { get; private init; } = string.Empty;

    /// <summary>
    /// URL миниатюры
    /// </summary>
    public string? ThumbnailUrl { get; private init; }

    /// <summary>
    /// Ширина в пикселях
    /// </summary>
    public int Width { get; private init; }

    /// <summary>
    /// Высота в пикселях
    /// </summary>
    public int Height { get; private init; }

    /// <summary>
    /// Размер файла в байтах
    /// </summary>
    public long FileSizeBytes { get; private init; }

    /// <summary>
    /// Формат изображения
    /// </summary>
    public ImageFormat Format { get; private init; } = ImageFormat.Png;

    /// <summary>
    /// Путь в Blob Storage (Azure/S3)
    /// </summary>
    public string? BlobPath { get; private init; }

    /// <summary>
    /// Соотношение сторон
    /// </summary>
    public string AspectRatio => Width > 0 && Height > 0 
        ? $"{Width}:{Height}" 
        : "unknown";

    /// <summary>
    /// Размер в человекочитаемом формате
    /// </summary>
    public string FileSizeFormatted
    {
        get
        {
            if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
            if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F1} KB";
            return $"{FileSizeBytes / (1024.0 * 1024.0):F1} MB";
        }
    }

    public static ImageMetadata Create(
        string url,
        int width,
        int height,
        long fileSizeBytes,
        ImageFormat format,
        string? thumbnailUrl = null,
        string? blobPath = null)
    {
        Guard.Against.NullOrWhiteSpace(url, nameof(url));
        Guard.Against.NegativeOrZero(width, nameof(width));
        Guard.Against.NegativeOrZero(height, nameof(height));
        Guard.Against.Negative(fileSizeBytes, nameof(fileSizeBytes));

        return new ImageMetadata(
            url,
            thumbnailUrl,
            width,
            height,
            fileSizeBytes,
            format,
            blobPath);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Url;
        yield return Width;
        yield return Height;
        yield return FileSizeBytes;
        yield return Format;
    }
}
