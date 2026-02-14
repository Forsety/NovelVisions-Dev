using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.Events;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;

/// <summary>
/// Aggregate Root: Задание на визуализацию
/// </summary>
public sealed class VisualizationJob : AggregateRoot<VisualizationJobId>
{
    private readonly List<GeneratedImage> _images = new();
    private VisualizationJobStatus _status = VisualizationJobStatus.Pending;
    private PromptData? _promptData;
    private string? _errorMessage;
    private int _retryCount;

    // Private constructor for EF Core
    private VisualizationJob() : base(default!) { }

    private VisualizationJob(
        VisualizationJobId id,
        Guid bookId,
        Guid? pageId,
        Guid? chapterId,
        Guid userId,
        VisualizationTrigger trigger,
        AIModelProvider preferredProvider,
        GenerationParameters parameters,
        TextSelection? textSelection,
        int priority) : base(id)
    {
        BookId = bookId;
        PageId = pageId;
        ChapterId = chapterId;
        UserId = userId;
        Trigger = trigger;
        PreferredProvider = preferredProvider;
        Parameters = parameters;
        TextSelection = textSelection;
        Priority = priority;
    }

    #region Properties

    /// <summary>
    /// ID книги
    /// </summary>
    public Guid BookId { get; private init; }

    /// <summary>
    /// ID страницы (если визуализация по странице)
    /// </summary>
    public Guid? PageId { get; private init; }

    /// <summary>
    /// ID главы (если визуализация по главе)
    /// </summary>
    public Guid? ChapterId { get; private init; }

    /// <summary>
    /// ID пользователя инициировавшего визуализацию
    /// </summary>
    public Guid UserId { get; private init; }

    /// <summary>
    /// Тип триггера визуализации
    /// </summary>
    public VisualizationTrigger Trigger { get; private init; } = VisualizationTrigger.Button;

    /// <summary>
    /// Текущий статус
    /// </summary>
    public VisualizationJobStatus Status => _status;

    /// <summary>
    /// Предпочитаемый AI провайдер
    /// </summary>
    public AIModelProvider PreferredProvider { get; private init; } = AIModelProvider.DallE3;

    /// <summary>
    /// Параметры генерации
    /// </summary>
    public GenerationParameters Parameters { get; private init; } = GenerationParameters.Default();

    /// <summary>
    /// Данные выделенного текста (для TextSelection триггера)
    /// </summary>
    public TextSelection? TextSelection { get; private init; }

    /// <summary>
    /// Данные промпта (заполняется после вызова PromptGen.API)
    /// </summary>
    public PromptData? PromptData => _promptData;

    /// <summary>
    /// Сгенерированные изображения
    /// </summary>
    public IReadOnlyList<GeneratedImage> Images => _images.AsReadOnly();

    /// <summary>
    /// Приоритет в очереди (выше = важнее)
    /// </summary>
    public int Priority { get; private init; }

    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>
    /// Количество повторных попыток
    /// </summary>
    public int RetryCount => _retryCount;

    /// <summary>
    /// Время начала обработки
    /// </summary>
    public DateTime? ProcessingStartedAt { get; private set; }

    /// <summary>
    /// Время завершения
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Внешний ID задания (от AI провайдера)
    /// </summary>
    public string? ExternalJobId { get; private set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Есть ли сгенерированные изображения
    /// </summary>
    public bool HasImages => _images.Any(i => !i.IsDeleted);

    /// <summary>
    /// Выбранное изображение
    /// </summary>
    public GeneratedImage? SelectedImage => _images.FirstOrDefault(i => i.IsSelected && !i.IsDeleted);

    /// <summary>
    /// Время обработки
    /// </summary>
    public TimeSpan? ProcessingTime => ProcessingStartedAt.HasValue && CompletedAt.HasValue
        ? CompletedAt.Value - ProcessingStartedAt.Value
        : null;

    /// <summary>
    /// Можно ли отменить
    /// </summary>
    public bool CanCancel => _status.CanCancel;

    /// <summary>
    /// Можно ли повторить
    /// </summary>
    public bool CanRetry => _status.CanRetry && _retryCount < 3;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Создать задание для кнопки "Визуализируй"
    /// </summary>
    public static Result<VisualizationJob> CreateForButton(
        Guid bookId,
        Guid pageId,
        Guid userId,
        AIModelProvider preferredProvider,
        GenerationParameters? parameters = null)
    {
        return Create(
            bookId,
            pageId,
            chapterId: null,
            userId,
            VisualizationTrigger.Button,
            preferredProvider,
            parameters ?? GenerationParameters.Default(),
            textSelection: null,
            priority: 10);
    }

    /// <summary>
    /// Создать задание для выделения текста
    /// </summary>
    public static Result<VisualizationJob> CreateForTextSelection(
        Guid bookId,
        TextSelection textSelection,
        Guid userId,
        AIModelProvider preferredProvider,
        GenerationParameters? parameters = null)
    {
        Guard.Against.Null(textSelection, nameof(textSelection));

        return Create(
            bookId,
            textSelection.PageId,
            textSelection.ChapterId,
            userId,
            VisualizationTrigger.TextSelection,
            preferredProvider,
            parameters ?? GenerationParameters.Default(),
            textSelection,
            priority: 15); // Higher priority for user-initiated
    }

    /// <summary>
    /// Создать задание для авто веб-новеллы
    /// </summary>
    public static Result<VisualizationJob> CreateForAutoNovel(
        Guid bookId,
        Guid pageId,
        Guid? chapterId,
        Guid userId,
        AIModelProvider preferredProvider,
        GenerationParameters? parameters = null)
    {
        return Create(
            bookId,
            pageId,
            chapterId,
            userId,
            VisualizationTrigger.AutoNovel,
            preferredProvider,
            parameters ?? GenerationParameters.Default(),
            textSelection: null,
            priority: 5); // Lower priority for batch processing
    }

    /// <summary>
    /// Создать задание для авторских мест
    /// </summary>
    public static Result<VisualizationJob> CreateForAuthorDefined(
        Guid bookId,
        Guid pageId,
        Guid? chapterId,
        Guid authorId,
        AIModelProvider preferredProvider,
        GenerationParameters? parameters = null)
    {
        return Create(
            bookId,
            pageId,
            chapterId,
            authorId,
            VisualizationTrigger.AuthorDefined,
            preferredProvider,
            parameters ?? GenerationParameters.Default(),
            textSelection: null,
            priority: 8);
    }

    private static Result<VisualizationJob> Create(
        Guid bookId,
        Guid? pageId,
        Guid? chapterId,
        Guid userId,
        VisualizationTrigger trigger,
        AIModelProvider preferredProvider,
        GenerationParameters parameters,
        TextSelection? textSelection,
        int priority)
    {
        try
        {
            Guard.Against.Default(bookId, nameof(bookId));
            Guard.Against.Default(userId, nameof(userId));

            var job = new VisualizationJob(
                VisualizationJobId.Create(),
                bookId,
                pageId,
                chapterId,
                userId,
                trigger,
                preferredProvider,
                parameters,
                textSelection,
                priority);

            job.AddDomainEvent(new VisualizationRequestedEvent(
                job.Id,
                bookId,
                pageId,
                chapterId,
                trigger,
                userId));

            return Result<VisualizationJob>.Success(job);
        }
        catch (Exception ex)
        {
            return Result<VisualizationJob>.Failure(Error.Validation(ex.Message));
        }
    }

    #endregion

    #region State Transitions

    /// <summary>
    /// Поставить в очередь
    /// </summary>
    public Result<bool> Enqueue(int queuePosition, TimeSpan estimatedWaitTime)
    {
        if (_status != VisualizationJobStatus.Pending)
        {
            return Result<bool>.Failure(Error.Validation(
                $"Cannot enqueue job in status {_status.Name}"));
        }

        _status = VisualizationJobStatus.Queued;
        IncrementVersion();

        AddDomainEvent(new VisualizationQueuedEvent(Id, queuePosition, estimatedWaitTime));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Начать генерацию промпта
    /// </summary>
    public Result<bool> StartPromptGeneration(string originalText)
    {
        if (_status != VisualizationJobStatus.Queued && _status != VisualizationJobStatus.Pending)
        {
            return Result<bool>.Failure(Error.Validation(
                $"Cannot start prompt generation in status {_status.Name}"));
        }

        _status = VisualizationJobStatus.GeneratingPrompt;
        ProcessingStartedAt = DateTime.UtcNow;
        IncrementVersion();

        AddDomainEvent(new PromptGenerationStartedEvent(Id, originalText, PreferredProvider));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Установить сгенерированный промпт
    /// </summary>
    public Result<bool> SetPromptData(PromptData promptData)
    {
        Guard.Against.Null(promptData, nameof(promptData));

        if (_status != VisualizationJobStatus.GeneratingPrompt)
        {
            return Result<bool>.Failure(Error.Validation(
                $"Cannot set prompt data in status {_status.Name}"));
        }

        _promptData = promptData;
        IncrementVersion();

        AddDomainEvent(new PromptGeneratedEvent(
            Id, 
            promptData.EnhancedPrompt, 
            promptData.NegativePrompt));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Начать обработку AI моделью
    /// </summary>
    public Result<bool> StartAIProcessing(string externalJobId)
    {
        if (_status != VisualizationJobStatus.GeneratingPrompt || _promptData == null)
        {
            return Result<bool>.Failure(Error.Validation(
                $"Cannot start AI processing in status {_status.Name} or without prompt data"));
        }

        _status = VisualizationJobStatus.Processing;
        ExternalJobId = externalJobId;
        IncrementVersion();

        AddDomainEvent(new AIProcessingStartedEvent(
            Id, 
            PreferredProvider, 
            _promptData.EnhancedPrompt));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// AI обработка завершена, начать загрузку
    /// </summary>
    public Result<bool> StartUploading()
    {
        if (_status != VisualizationJobStatus.Processing)
        {
            return Result<bool>.Failure(Error.Validation(
                $"Cannot start uploading in status {_status.Name}"));
        }

        _status = VisualizationJobStatus.Uploading;
        IncrementVersion();

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Добавить сгенерированное изображение
    /// </summary>
    public Result<GeneratedImage> AddImage(
        ImageMetadata metadata,
        string? externalJobId = null)
    {
        if (_promptData == null)
        {
            return Result<GeneratedImage>.Failure(Error.Validation("Prompt data not set"));
        }

        var image = GeneratedImage.Create(
            Id,
            metadata,
            _promptData,
            PreferredProvider,
            externalJobId ?? ExternalJobId);

        _images.Add(image);

        // Первое изображение автоматически выбирается
        if (_images.Count == 1)
        {
            image.Select();
        }

        IncrementVersion();

        AddDomainEvent(new ImageUploadedEvent(
            Id,
            image.Id,
            metadata.Url,
            metadata.ThumbnailUrl));

        return Result<GeneratedImage>.Success(image);
    }

    /// <summary>
    /// Завершить успешно
    /// </summary>
    public Result<bool> Complete()
    {
        if (!HasImages)
        {
            return Result<bool>.Failure(Error.Validation("Cannot complete without images"));
        }

        _status = VisualizationJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        _errorMessage = null;
        IncrementVersion();

        var selectedImage = SelectedImage ?? _images.First(i => !i.IsDeleted);

        AddDomainEvent(new VisualizationCompletedEvent(
            Id,
            selectedImage.Id,
            BookId,
            PageId,
            selectedImage.ImageUrl,
            ProcessingTime ?? TimeSpan.Zero));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Отметить как неудачное
    /// </summary>
    public Result<bool> Fail(string errorMessage, string? errorCode = null)
    {
        Guard.Against.NullOrWhiteSpace(errorMessage, nameof(errorMessage));

        var previousStatus = _status;
        _status = VisualizationJobStatus.Failed;
        _errorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        IncrementVersion();

        AddDomainEvent(new VisualizationFailedEvent(
            Id,
            errorMessage,
            errorCode,
            previousStatus,
            _retryCount));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Отменить
    /// </summary>
    public Result<bool> Cancel(string reason, Guid? cancelledByUserId = null)
    {
        if (!CanCancel)
        {
            return Result<bool>.Failure(Error.Validation(
                $"Cannot cancel job in status {_status.Name}"));
        }

        _status = VisualizationJobStatus.Cancelled;
        _errorMessage = reason;
        CompletedAt = DateTime.UtcNow;
        IncrementVersion();

        AddDomainEvent(new VisualizationCancelledEvent(Id, reason, cancelledByUserId));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Повторить попытку
    /// </summary>
    public Result<bool> Retry()
    {
        if (!CanRetry)
        {
            return Result<bool>.Failure(Error.Validation(
                $"Cannot retry job in status {_status.Name} or max retries exceeded"));
        }

        var previousError = _errorMessage;
        _retryCount++;
        _status = VisualizationJobStatus.Pending;
        _errorMessage = null;
        ProcessingStartedAt = null;
        CompletedAt = null;
        ExternalJobId = null;
        IncrementVersion();

        AddDomainEvent(new VisualizationRetryEvent(Id, _retryCount, previousError ?? "Unknown"));

        return Result<bool>.Success(true);
    }

    #endregion

    #region Image Management

    /// <summary>
    /// Выбрать изображение
    /// </summary>
    public Result<bool> SelectImage(GeneratedImageId imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId && !i.IsDeleted);
        if (image == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Image {imageId} not found"));
        }

        // Deselect all others
        foreach (var img in _images.Where(i => i.IsSelected))
        {
            img.Deselect();
        }

        image.Select();
        IncrementVersion();

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Удалить изображение
    /// </summary>
    public Result<bool> DeleteImage(GeneratedImageId imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId && !i.IsDeleted);
        if (image == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Image {imageId} not found"));
        }

        image.Delete();

        // If deleted image was selected, select another one
        if (!_images.Any(i => i.IsSelected && !i.IsDeleted))
        {
            var nextImage = _images.FirstOrDefault(i => !i.IsDeleted);
            nextImage?.Select();
        }

        IncrementVersion();

        return Result<bool>.Success(true);
    }

    #endregion
}
