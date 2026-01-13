// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/DTOs/Import/ImportBookResultDto.cs
using System;
using System.Collections.Generic;

namespace NovelVision.Services.Catalog.Application.DTOs.Import;

/// <summary>
/// DTO для результата импорта одной книги
/// </summary>
public record ImportBookResultDto
{
    /// <summary>
    /// Успешно ли выполнен импорт
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// ID созданной книги
    /// </summary>
    public Guid? BookId { get; init; }

    /// <summary>
    /// ID книги в Gutenberg
    /// </summary>
    public int GutenbergId { get; init; }

    /// <summary>
    /// Название книги
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Имя автора
    /// </summary>
    public string? AuthorName { get; init; }

    /// <summary>
    /// ID автора
    /// </summary>
    public Guid? AuthorId { get; init; }

    /// <summary>
    /// Создан ли новый автор
    /// </summary>
    public bool AuthorCreated { get; init; }

    /// <summary>
    /// Это новая книга (не обновление)
    /// </summary>
    public bool IsNewBook { get; init; }

    /// <summary>
    /// Это новый автор
    /// </summary>
    public bool IsNewAuthor { get; init; }

    /// <summary>
    /// Количество созданных глав
    /// </summary>
    public int ChaptersCreated { get; init; }

    /// <summary>
    /// Количество импортированных глав (алиас для ChaptersCreated)
    /// </summary>
    public int ChaptersImported { get => ChaptersCreated; init => ChaptersCreated = value; }

    /// <summary>
    /// Количество созданных страниц
    /// </summary>
    public int PagesCreated { get; init; }

    /// <summary>
    /// Количество импортированных страниц (алиас для PagesCreated)
    /// </summary>
    public int PagesImported { get => PagesCreated; init => PagesCreated = value; }

    /// <summary>
    /// Количество слов в книге
    /// </summary>
    public int WordCount { get; init; }

    /// <summary>
    /// Есть ли обложка
    /// </summary>
    public bool HasCover { get; init; }

    /// <summary>
    /// URL обложки
    /// </summary>
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Назначенные категории/темы
    /// </summary>
    public List<string> SubjectsAssigned { get; init; } = new();

    /// <summary>
    /// Темы книги (алиас для SubjectsAssigned)
    /// </summary>
    public List<string> Subjects { get => SubjectsAssigned; init => SubjectsAssigned = value; }

    /// <summary>
    /// Предупреждения при импорте
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Длительность импорта
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Длительность импорта (алиас для Duration)
    /// </summary>
    public TimeSpan ImportDuration { get => Duration; init => Duration = value; }
}

/// <summary>
/// DTO для результата массового импорта
/// </summary>
public record BulkImportResultDto
{
    /// <summary>
    /// Общее количество запрошенных для импорта книг
    /// </summary>
    public int TotalRequested { get; init; }

    /// <summary>
    /// Количество успешно импортированных
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Количество успешных импортов (алиас)
    /// </summary>
    public int SuccessfulImports { get => SuccessCount; init => SuccessCount = value; }

    /// <summary>
    /// Количество неудачных импортов
    /// </summary>
    public int FailedCount { get; init; }

    /// <summary>
    /// Количество провальных импортов (алиас)
    /// </summary>
    public int FailedImports { get => FailedCount; init => FailedCount = value; }

    /// <summary>
    /// Количество пропущенных (уже существующих)
    /// </summary>
    public int SkippedCount { get; init; }

    /// <summary>
    /// Количество пропущенных существующих (алиас)
    /// </summary>
    public int SkippedExisting { get => SkippedCount; init => SkippedCount = value; }

    /// <summary>
    /// Количество созданных авторов
    /// </summary>
    public int AuthorsCreated { get; init; }

    /// <summary>
    /// Количество созданных категорий
    /// </summary>
    public int SubjectsCreated { get; init; }

    /// <summary>
    /// Результаты по каждой книге
    /// </summary>
    public List<ImportBookResultDto> Results { get; init; } = new();

    /// <summary>
    /// Импортированные книги (алиас для Results)
    /// </summary>
    public List<ImportBookResultDto> ImportedBooks { get => Results; init => Results = value; }

    /// <summary>
    /// Ошибки импорта
    /// </summary>
    public List<ImportErrorDto> Errors { get; init; } = new();

    /// <summary>
    /// Общая длительность импорта
    /// </summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>
    /// Время начала
    /// </summary>
    public DateTime StartedAt { get; init; }

    /// <summary>
    /// Время завершения
    /// </summary>
    public DateTime CompletedAt { get; init; }
}

/// <summary>
/// DTO для ошибки импорта
/// </summary>
public record ImportErrorDto
{
    /// <summary>
    /// ID книги в Gutenberg
    /// </summary>
    public int GutenbergId { get; init; }

    /// <summary>
    /// Название книги
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Код ошибки
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Время возникновения ошибки
    /// </summary>
    public DateTime OccurredAt { get; init; }
}

/// <summary>
/// DTO для статуса импорта (для отслеживания прогресса)
/// </summary>
public record ImportProgressDto
{
    public Guid JobId { get; init; }
    public string Status { get; init; } = string.Empty;
    public int TotalBooks { get; init; }
    public int ProcessedBooks { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public double ProgressPercent { get; init; }
    public string? CurrentBook { get; init; }
    public TimeSpan ElapsedTime { get; init; }
    public TimeSpan? EstimatedTimeRemaining { get; init; }
}