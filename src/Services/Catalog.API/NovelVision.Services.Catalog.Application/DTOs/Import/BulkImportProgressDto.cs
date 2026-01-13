// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/Import/BulkImportProgressDto.cs
using System;

namespace NovelVision.Services.Catalog.Application.DTOs.Import;

/// <summary>
/// DTO для отслеживания прогресса массового импорта
/// </summary>
public record BulkImportProgressDto
{
    /// <summary>
    /// Текущий индекс обрабатываемой книги (1-based)
    /// </summary>
    public int Current { get; init; }

    /// <summary>
    /// Общее количество книг для импорта
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// Количество успешно импортированных книг
    /// </summary>
    public int Successful { get; init; }

    /// <summary>
    /// Количество неудачных попыток импорта
    /// </summary>
    public int Failed { get; init; }

    /// <summary>
    /// Количество пропущенных (уже существующих) книг
    /// </summary>
    public int Skipped { get; init; }

    /// <summary>
    /// Название текущей обрабатываемой книги
    /// </summary>
    public string? CurrentBookTitle { get; init; }

    /// <summary>
    /// Gutenberg ID текущей книги
    /// </summary>
    public int? CurrentGutenbergId { get; init; }

    /// <summary>
    /// Процент выполнения (0-100)
    /// </summary>
    public double ProgressPercent => Total > 0 ? Math.Round((double)Current / Total * 100, 2) : 0;

    /// <summary>
    /// Время начала импорта
    /// </summary>
    public DateTime? StartedAt { get; init; }

    /// <summary>
    /// Прошедшее время
    /// </summary>
    public TimeSpan? ElapsedTime { get; init; }

    /// <summary>
    /// Оценочное оставшееся время
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; init; }

    /// <summary>
    /// Статус текущей операции
    /// </summary>
    public string Status { get; init; } = "Processing";
}