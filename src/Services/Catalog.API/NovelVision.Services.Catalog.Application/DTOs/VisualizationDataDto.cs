// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/VisualizationDataDto.cs
namespace NovelVision.Services.Catalog.Application.DTOs;

/// <summary>
/// DTO для даних візуалізації сторінки
/// </summary>
public sealed record VisualizationDataDto
{
    /// <summary>
    /// URL основного зображення
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// URL мініатюри
    /// </summary>
    public string? ThumbnailUrl { get; init; }

    /// <summary>
    /// ID завдання візуалізації
    /// </summary>
    public Guid? JobId { get; init; }

    /// <summary>
    /// Чи є точкою візуалізації
    /// </summary>
    public bool IsVisualizationPoint { get; init; }

    /// <summary>
    /// Підказка автора для візуалізації
    /// </summary>
    public string? AuthorHint { get; init; }

    /// <summary>
    /// Дата генерації
    /// </summary>
    public DateTime? GeneratedAt { get; init; }

    /// <summary>
    /// Чи є візуалізація
    /// </summary>
    public bool HasVisualization => !string.IsNullOrEmpty(ImageUrl);
}