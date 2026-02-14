// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Entities/Page.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.Events;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Domain.Entities;

/// <summary>
/// Страница книги с поддержкой визуализации
/// </summary>
public sealed class Page : Entity<PageId>
{
    private string _content = string.Empty;
    private readonly List<string> _visualizationPrompts = new();

    #region Constructors

    /// <summary>
    /// Private parameterless constructor for EF Core
    /// </summary>
    private Page() : base(default!)
    {
        // EF Core will set all properties via reflection
        // Инициализируем поля значениями по умолчанию для избежания CS8618
        _content = string.Empty;
        ChapterId = default!;
    }

    private Page(
        PageId id,
        int pageNumber,
        string content,
        ChapterId chapterId) : base(id)
    {
        PageNumber = pageNumber;
        _content = content;
        ChapterId = chapterId;
    }

    #endregion

    #region Core Properties

    /// <summary>
    /// Номер страницы в главе (начиная с 1)
    /// </summary>
    public int PageNumber { get; private set; }

    /// <summary>
    /// Текстовый контент страницы
    /// </summary>
    public string Content => _content;

    /// <summary>
    /// ID главы, к которой принадлежит страница
    /// </summary>
    public ChapterId ChapterId { get; private set; }

    /// <summary>
    /// Промпты для визуализации (legacy, сохраняем для совместимости)
    /// </summary>
    public IReadOnlyList<string> VisualizationPrompts => _visualizationPrompts.AsReadOnly();

    #endregion

    #region Visualization Properties

    /// <summary>
    /// URL основного изображения визуализации
    /// </summary>
    public string? VisualizationImageUrl { get; private set; }

    /// <summary>
    /// URL миниатюры изображения
    /// </summary>
    public string? VisualizationThumbnailUrl { get; private set; }

    /// <summary>
    /// ID задания визуализации из Visualization.API
    /// </summary>
    public Guid? VisualizationJobId { get; private set; }

    /// <summary>
    /// Помечена ли страница автором как точка визуализации
    /// Используется в режиме AuthorDefined
    /// </summary>
    public bool IsVisualizationPoint { get; private set; }

    /// <summary>
    /// Подсказка автора для визуализации
    /// Например: "Сцена в тронном зале, золотой свет, король на троне"
    /// </summary>
    public string? AuthorVisualizationHint { get; private set; }

    /// <summary>
    /// Дата и время генерации текущей визуализации
    /// </summary>
    public DateTime? VisualizationGeneratedAt { get; private set; }

    /// <summary>
    /// Статус генерации визуализации
    /// </summary>
    public VisualizationStatus VisualizationStatus { get; private set; } = VisualizationStatus.None;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Количество слов на странице
    /// </summary>
    public int WordCount => string.IsNullOrWhiteSpace(_content)
        ? 0
        : _content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>
    /// Примерное время чтения страницы (средняя скорость 250 слов/мин)
    /// </summary>
    public TimeSpan EstimatedReadingTime => TimeSpan.FromMinutes(WordCount / 250.0);

    /// <summary>
    /// Есть ли визуализация на странице
    /// </summary>
    public bool HasVisualization => !string.IsNullOrEmpty(VisualizationImageUrl);

    /// <summary>
    /// Количество символов на странице
    /// </summary>
    public int CharacterCount => _content?.Length ?? 0;

    /// <summary>
    /// Ожидает ли страница генерации визуализации
    /// </summary>
    public bool IsPendingVisualization => VisualizationStatus == VisualizationStatus.Pending ||
                                           VisualizationStatus == VisualizationStatus.Processing;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Создает новую страницу
    /// </summary>
    public static Page Create(
        int pageNumber,
        string content,
        ChapterId chapterId)
    {
        Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.Null(chapterId, nameof(chapterId));

        return new Page(
            PageId.Create(),
            pageNumber,
            content,
            chapterId);
    }

    /// <summary>
    /// Создает страницу с указанным ID (для импорта)
    /// </summary>
    public static Page CreateWithId(
        PageId id,
        int pageNumber,
        string content,
        ChapterId chapterId)
    {
        Guard.Against.Null(id, nameof(id));
        Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
        Guard.Against.NullOrWhiteSpace(content, nameof(content));
        Guard.Against.Null(chapterId, nameof(chapterId));

        return new Page(id, pageNumber, content, chapterId);
    }

    #endregion

    #region Content Methods

    /// <summary>
    /// Обновляет контент страницы
    /// </summary>
    public void UpdateContent(string newContent)
    {
        Guard.Against.NullOrWhiteSpace(newContent, nameof(newContent));

        var oldWordCount = WordCount;
        _content = newContent;
        UpdateTimestamp();

        // Если контент изменился значительно, сбрасываем визуализацию
        if (HasVisualization && Math.Abs(WordCount - oldWordCount) > oldWordCount * 0.3)
        {
            ClearVisualization();
        }

        AddDomainEvent(new PageContentUpdatedEvent(
            Id, ChapterId, oldWordCount, WordCount));
    }

    /// <summary>
    /// Устанавливает номер страницы (используется при переупорядочивании)
    /// </summary>
    internal void SetPageNumber(int pageNumber)
    {
        Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
        PageNumber = pageNumber;
        UpdateTimestamp();
    }

    #endregion

    #region Visualization Methods

    /// <summary>
    /// Устанавливает визуализацию на страницу
    /// </summary>
    /// <param name="imageUrl">URL полноразмерного изображения</param>
    /// <param name="thumbnailUrl">URL миниатюры (опционально)</param>
    /// <param name="jobId">ID задания визуализации (опционально)</param>
    public void SetVisualization(
        string imageUrl,
        string? thumbnailUrl = null,
        Guid? jobId = null)
    {
        Guard.Against.NullOrWhiteSpace(imageUrl, nameof(imageUrl));

        VisualizationImageUrl = imageUrl;
        VisualizationThumbnailUrl = thumbnailUrl ?? GenerateThumbnailUrl(imageUrl);
        VisualizationJobId = jobId;
        VisualizationGeneratedAt = DateTime.UtcNow;
        VisualizationStatus = VisualizationStatus.Completed;
        UpdateTimestamp();

        AddDomainEvent(new PageVisualizationSetEvent(Id, ChapterId, imageUrl));
    }

    /// <summary>
    /// Запускает процесс генерации визуализации
    /// </summary>
    public void StartVisualizationGeneration(Guid jobId)
    {
        VisualizationJobId = jobId;
        VisualizationStatus = VisualizationStatus.Processing;
        UpdateTimestamp();

        AddDomainEvent(new PageVisualizationStartedEvent(Id, ChapterId, jobId));
    }

    /// <summary>
    /// Устанавливает статус ожидания генерации
    /// </summary>
    public void SetPendingVisualization()
    {
        VisualizationStatus = VisualizationStatus.Pending;
        UpdateTimestamp();
    }

    /// <summary>
    /// Устанавливает ошибку генерации визуализации
    /// </summary>
    public void SetVisualizationFailed(string? errorMessage = null)
    {
        VisualizationStatus = VisualizationStatus.Failed;
        UpdateTimestamp();

        AddDomainEvent(new PageVisualizationFailedEvent(Id, ChapterId, errorMessage));
    }

    /// <summary>
    /// Очищает визуализацию страницы
    /// </summary>
    public void ClearVisualization()
    {
        var hadVisualization = HasVisualization;

        VisualizationImageUrl = null;
        VisualizationThumbnailUrl = null;
        VisualizationJobId = null;
        VisualizationGeneratedAt = null;
        VisualizationStatus = VisualizationStatus.None;
        UpdateTimestamp();

        if (hadVisualization)
        {
            AddDomainEvent(new PageVisualizationClearedEvent(Id, ChapterId));
        }
    }

    /// <summary>
    /// Помечает страницу как точку визуализации (для режима AuthorDefined)
    /// </summary>
    public void MarkAsVisualizationPoint(string? hint = null)
    {
        IsVisualizationPoint = true;
        AuthorVisualizationHint = hint?.Trim();
        UpdateTimestamp();

        AddDomainEvent(new PageMarkedAsVisualizationPointEvent(Id, ChapterId, hint));
    }

    /// <summary>
    /// Снимает отметку точки визуализации
    /// </summary>
    public void UnmarkAsVisualizationPoint()
    {
        IsVisualizationPoint = false;
        AuthorVisualizationHint = null;
        UpdateTimestamp();
    }

    /// <summary>
    /// Обновляет подсказку для визуализации
    /// </summary>
    public void UpdateVisualizationHint(string? hint)
    {
        AuthorVisualizationHint = hint?.Trim();
        UpdateTimestamp();
    }

    /// <summary>
    /// Добавляет промпт визуализации (legacy)
    /// </summary>
    [Obsolete("Use SetVisualization instead")]
    public void AddVisualizationPrompt(string prompt)
    {
        Guard.Against.NullOrWhiteSpace(prompt, nameof(prompt));

        if (_visualizationPrompts.Count >= 5)
        {
            throw new InvalidOperationException("Maximum 5 visualization prompts allowed per page");
        }

        _visualizationPrompts.Add(prompt.Trim());
        UpdateTimestamp();
    }

    /// <summary>
    /// Очищает промпты визуализации (legacy)
    /// </summary>
    [Obsolete("Use ClearVisualization instead")]
    public void ClearVisualizationPrompts()
    {
        _visualizationPrompts.Clear();
        UpdateTimestamp();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Генерирует URL миниатюры на основе URL изображения
    /// </summary>
    private static string GenerateThumbnailUrl(string imageUrl)
    {
        // Простая логика для генерации URL миниатюры
        // В реальности это должно обрабатываться на уровне CDN/Storage
        if (imageUrl.Contains("?"))
        {
            return imageUrl + "&thumbnail=true";
        }
        return imageUrl + "?thumbnail=true";
    }

    #endregion
}

/// <summary>
/// Статус генерации визуализации страницы
/// </summary>
public enum VisualizationStatus
{
    /// <summary>
    /// Визуализация отсутствует
    /// </summary>
    None = 0,

    /// <summary>
    /// Ожидает генерации
    /// </summary>
    Pending = 1,

    /// <summary>
    /// В процессе генерации
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Успешно сгенерирована
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Ошибка генерации
    /// </summary>
    Failed = 4
}