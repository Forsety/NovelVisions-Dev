using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Visualization.Domain.ValueObjects;

/// <summary>
/// Выделенный текст пользователем для визуализации
/// </summary>
public sealed class TextSelection : ValueObject
{
    private TextSelection() { }

    private TextSelection(
        string selectedText,
        int startPosition,
        int endPosition,
        Guid pageId,
        Guid? chapterId,
        string? contextBefore,
        string? contextAfter)
    {
        SelectedText = selectedText;
        StartPosition = startPosition;
        EndPosition = endPosition;
        PageId = pageId;
        ChapterId = chapterId;
        ContextBefore = contextBefore;
        ContextAfter = contextAfter;
    }

    /// <summary>
    /// Выделенный текст
    /// </summary>
    public string SelectedText { get; private init; } = string.Empty;

    /// <summary>
    /// Начальная позиция выделения в тексте страницы
    /// </summary>
    public int StartPosition { get; private init; }

    /// <summary>
    /// Конечная позиция выделения в тексте страницы
    /// </summary>
    public int EndPosition { get; private init; }

    /// <summary>
    /// ID страницы где было выделение
    /// </summary>
    public Guid PageId { get; private init; }

    /// <summary>
    /// ID главы (опционально)
    /// </summary>
    public Guid? ChapterId { get; private init; }

    /// <summary>
    /// Контекст перед выделением (для лучшего промпта)
    /// </summary>
    public string? ContextBefore { get; private init; }

    /// <summary>
    /// Контекст после выделения
    /// </summary>
    public string? ContextAfter { get; private init; }

    /// <summary>
    /// Длина выделения в символах
    /// </summary>
    public int Length => EndPosition - StartPosition;

    /// <summary>
    /// Полный контекст (before + selected + after)
    /// </summary>
    public string FullContext => 
        $"{ContextBefore ?? ""}{SelectedText}{ContextAfter ?? ""}".Trim();

    public static TextSelection Create(
        string selectedText,
        int startPosition,
        int endPosition,
        Guid pageId,
        Guid? chapterId = null,
        string? contextBefore = null,
        string? contextAfter = null)
    {
        Guard.Against.NullOrWhiteSpace(selectedText, nameof(selectedText));
        Guard.Against.Negative(startPosition, nameof(startPosition));
        Guard.Against.NegativeOrZero(endPosition, nameof(endPosition));
        
        if (startPosition >= endPosition)
        {
            throw new ArgumentException("Start position must be less than end position");
        }

        return new TextSelection(
            selectedText,
            startPosition,
            endPosition,
            pageId,
            chapterId,
            contextBefore?.Length > 200 ? contextBefore[^200..] : contextBefore,
            contextAfter?.Length > 200 ? contextAfter[..200] : contextAfter);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SelectedText;
        yield return StartPosition;
        yield return EndPosition;
        yield return PageId;
    }
}
