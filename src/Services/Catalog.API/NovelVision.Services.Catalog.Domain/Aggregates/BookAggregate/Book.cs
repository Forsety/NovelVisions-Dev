// =============================================================================
// ФАЙЛ: src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Aggregates/BookAggregate/Book.cs
// ПОЛНАЯ ВЕРСИЯ BOOK AGGREGATE с учётом всех исправлений
// ИСПРАВЛЕНО:
// 1. PublicationInfo.Create() вместо PublicationInfo.Empty
// 2. CopyrightStatus без default value (не константа)
// 3. Chapter.Create с 4 параметрами (без chapterId)
// 4. События с правильными сигнатурами
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Events;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;

/// <summary>
/// Агрегат книги - корневая сущность каталога
/// </summary>
public sealed class Book : AggregateRoot<BookId>
{
    #region Private Fields

    private readonly List<Chapter> _chapters = new();
    private readonly List<Subject> _bookSubjects = new();
    private readonly HashSet<string> _genres = new();
    private readonly HashSet<string> _tags = new();
    private readonly HashSet<SubjectId> _subjectIds = new();
    private BookMetadata _metadata = null!;
    private BookStatus _status = BookStatus.Draft;
    private VisualizationMode _visualizationMode = VisualizationMode.None;
    private VisualizationSettings _visualizationSettings = null!;
    private CoverImage _coverImage = null!;

    #endregion

    #region Constructors

    /// <summary>
    /// Private parameterless constructor for EF Core
    /// </summary>
    private Book() : base(default!)
    {
        // Инициализация для EF Core - все свойства будут установлены через reflection
        _metadata = BookMetadata.Empty;
        _visualizationSettings = VisualizationSettings.Default();
        AuthorId = default!;
        PublicationInfo = PublicationInfo.Create(); // ИСПРАВЛЕНО: Create() вместо Empty
        Statistics = BookStatistics.Empty;
        _coverImage = CoverImage.Empty;
        ExternalIds = null;
        Source = BookSource.UserCreated;
    }

    private Book(
        BookId id,
        BookMetadata metadata,
        AuthorId authorId,
        BookISBN? isbn = null,
        PublicationInfo? publicationInfo = null) : base(id)
    {
        _metadata = metadata;
        AuthorId = authorId;
        ISBN = isbn;
        PublicationInfo = publicationInfo ?? PublicationInfo.Create(); // ИСПРАВЛЕНО
        Statistics = BookStatistics.Empty;
        _coverImage = CoverImage.Empty;
        _visualizationSettings = VisualizationSettings.Default();
        Source = BookSource.UserCreated;
    }

    #endregion

    #region Core Properties

    /// <summary>
    /// Метаданные книги (название, описание, язык и т.д.)
    /// </summary>
    public BookMetadata Metadata => _metadata;

    /// <summary>
    /// ID автора книги
    /// </summary>
    public AuthorId AuthorId { get; private set; }

    /// <summary>
    /// ISBN книги (опционально)
    /// </summary>
    public BookISBN? ISBN { get; private set; }

    /// <summary>
    /// Информация о публикации
    /// </summary>
    public PublicationInfo PublicationInfo { get; private set; }

    /// <summary>
    /// Статистика книги (просмотры, оценки и т.д.)
    /// </summary>
    public BookStatistics Statistics { get; private set; }

    /// <summary>
    /// Обложка книги
    /// </summary>
    public CoverImage CoverImage => _coverImage;

    /// <summary>
    /// Глав книги
    /// </summary>
    public IReadOnlyList<Chapter> Chapters => _chapters.AsReadOnly();

    /// <summary>
    /// Темы/категории книги
    /// </summary>
    public IReadOnlyList<Subject> BookSubjects => _bookSubjects.AsReadOnly();



    /// <summary>
    /// Жанры книги
    /// </summary>
    public IReadOnlySet<string> Genres => _genres;

    /// <summary>
    /// Теги книги
    /// </summary>
    public IReadOnlySet<string> Tags => _tags;

    #endregion

    #region Status Properties

    /// <summary>
    /// Текущий статус книги
    /// </summary>
    public BookStatus Status => _status;

    /// <summary>
    /// Статус авторских прав
    /// </summary>
    public CopyrightStatus CopyrightStatus { get; private set; } = CopyrightStatus.Unknown;

    /// <summary>
    /// Режим визуализации
    /// </summary>
    public VisualizationMode VisualizationMode => _visualizationMode;

    /// <summary>
    /// Настройки визуализации
    /// </summary>
    public VisualizationSettings VisualizationSettings => _visualizationSettings;

    #endregion

    #region External Source Properties

    /// <summary>
    /// Внешние идентификаторы книги (Gutenberg, OpenLibrary и т.д.)
    /// </summary>
    public ExternalBookId? ExternalIds { get; private set; }

    /// <summary>
    /// Устаревшее свойство - используйте ExternalIds
    /// </summary>
    [Obsolete("Use ExternalIds instead")]
    public ExternalBookId? ExternalId => ExternalIds;

    /// <summary>
    /// ID тем/категорий книги (strongly typed)
    /// </summary>
    public IReadOnlySet<SubjectId> SubjectIds => _subjectIds;

    /// <summary>
    /// Количество скачиваний (для внешних источников)
    /// </summary>
    public int DownloadCount { get; private set; }

    /// <summary>
    /// Источник книги (UserCreated, Gutenberg, OpenLibrary и т.д.)
    /// </summary>
    public BookSource Source { get; private set; } = BookSource.UserCreated;

    #endregion

    #region Content Properties

    /// <summary>
    /// Год оригинальной публикации
    /// </summary>
    public int? OriginalPublicationYear { get; private set; }

    /// <summary>
    /// Общее количество слов
    /// </summary>
    public int WordCount { get; private set; }

    /// <summary>
    /// Сложность чтения (Flesch-Kincaid)
    /// </summary>
    public double? ReadingDifficulty { get; private set; }

    /// <summary>
    /// Ссылка на полный текст (для внешних источников)
    /// </summary>
    public string? FullTextUrl { get; private set; }

    /// <summary>
    /// Доступен ли полный текст
    /// </summary>
    public bool HasFullText { get; private set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Количество глав
    /// </summary>
    public int ChapterCount => _chapters.Count;

    /// <summary>
    /// Общее количество страниц
    /// </summary>
    public int TotalPageCount => _chapters.Sum(c => c.PageCount);

    /// <summary>
    /// Общее количество слов (рассчитанное или из WordCount)
    /// </summary>
    public int TotalWordCount => WordCount > 0 ? WordCount : _chapters.Sum(c => c.TotalWordCount);

    /// <summary>
    /// Примерное время чтения (250 слов/минуту)
    /// </summary>
    public TimeSpan EstimatedReadingTime => TimeSpan.FromMinutes(TotalWordCount / 250.0);

    /// <summary>
    /// Опубликована ли книга
    /// </summary>
    public bool IsPublished => _status == BookStatus.Published;

    /// <summary>
    /// Можно ли визуализировать книгу
    /// </summary>
    public bool CanBeVisualized => _visualizationSettings.IsEnabled && IsPublished;

    /// <summary>
    /// Включена ли визуализация
    /// </summary>
    public bool IsVisualizationEnabled => _visualizationSettings.IsEnabled;

    /// <summary>
    /// Книга из внешнего источника
    /// </summary>
    public bool IsFromExternalSource => ExternalIds != null && ExternalIds.SourceType != ExternalSourceType.Manual;

    /// <summary>
    /// Книга в общественном достоянии
    /// </summary>
    public bool IsPublicDomain => CopyrightStatus == CopyrightStatus.PublicDomain;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Создаёт новую книгу
    /// </summary>
    public static Result<Book> Create(
        BookMetadata metadata,
        AuthorId authorId,
        BookISBN? isbn = null,
        PublicationInfo? publicationInfo = null)
    {
        try
        {
            Guard.Against.Null(metadata, nameof(metadata));
            Guard.Against.Null(authorId, nameof(authorId));

            var book = new Book(
                BookId.Create(),
                metadata,
                authorId,
                isbn,
                publicationInfo);

            book.AddDomainEvent(new BookCreatedEvent(book.Id, book.Metadata.Title, book.AuthorId));

            return Result<Book>.Success(book);
        }
        catch (Exception ex)
        {
            return Result<Book>.Failure(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// Создаёт книгу из внешнего источника (Gutenberg, OpenLibrary и т.д.)
    /// </summary>
    public static Result<Book> CreateFromExternalSource(
        BookMetadata metadata,
        AuthorId authorId,
        ExternalBookId externalId,
        CoverImage? coverImage = null,
        int downloadCount = 0,
        CopyrightStatus? copyrightStatus = null) // ИСПРАВЛЕНО: nullable без default value
    {
        try
        {
            Guard.Against.Null(metadata, nameof(metadata));
            Guard.Against.Null(authorId, nameof(authorId));
            Guard.Against.Null(externalId, nameof(externalId));

            var book = new Book(
                BookId.Create(),
                metadata,
                authorId,
                null,  // ISBN обычно нет у Gutenberg
                PublicationInfo.Create()); // ИСПРАВЛЕНО

            book.ExternalIds = externalId;
            book.DownloadCount = downloadCount;
            book.Source = externalId.SourceType.ToBookSource();
            book._coverImage = coverImage ?? CoverImage.Empty;
            book.CopyrightStatus = copyrightStatus ?? CopyrightStatus.Unknown; // ИСПРАВЛЕНО

            book.AddDomainEvent(new BookImportedEvent(book.Id, externalId));

            return Result<Book>.Success(book);
        }
        catch (Exception ex)
        {
            return Result<Book>.Failure(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// Алиас для CreateFromExternalSource (для совместимости)
    /// </summary>
    public static Result<Book> CreateFromExternal(
        BookMetadata metadata,
        AuthorId authorId,
        ExternalBookId externalId,
        CoverImage? coverImage = null,
        int downloadCount = 0,
        CopyrightStatus? copyrightStatus = null)
    {
        return CreateFromExternalSource(metadata, authorId, externalId, coverImage, downloadCount, copyrightStatus);
    }

    #endregion

    #region Chapter Management

    /// <summary>
    /// Добавляет главу в книгу
    /// </summary>
    public Result<Chapter> AddChapter(string title, string? description = null)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(title, nameof(title));

            var orderIndex = _chapters.Count + 1;

            // ИСПРАВЛЕНО: Chapter.Create принимает 4 параметра (без chapterId)
            var chapter = Chapter.Create(title, orderIndex, Id, description);
            _chapters.Add(chapter);

            IncrementVersion();
            AddDomainEvent(new ChapterAddedEvent(Id, chapter.Id, title, orderIndex));

            return Result<Chapter>.Success(chapter);
        }
        catch (Exception ex)
        {
            return Result<Chapter>.Failure(Error.Validation(ex.Message));
        }
    }

    /// <summary>
    /// Удаляет главу из книги
    /// </summary>
    public Result<bool> RemoveChapter(ChapterId chapterId)
    {
        var chapter = _chapters.FirstOrDefault(c => c.Id == chapterId);
        if (chapter == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Chapter {chapterId} not found"));
        }

        _chapters.Remove(chapter);
        ReorderChapters();
        IncrementVersion();
        AddDomainEvent(new ChapterRemovedEvent(Id, chapterId));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Перемещает главу на новую позицию
    /// </summary>
    public Result<bool> ReorderChapter(ChapterId chapterId, int newOrderIndex)
    {
        var chapter = _chapters.FirstOrDefault(c => c.Id == chapterId);
        if (chapter == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Chapter {chapterId} not found"));
        }

        if (newOrderIndex < 1 || newOrderIndex > _chapters.Count)
        {
            return Result<bool>.Failure(Error.Validation("Invalid order index"));
        }

        _chapters.Remove(chapter);
        _chapters.Insert(newOrderIndex - 1, chapter);
        ReorderChapters();
        IncrementVersion();

        return Result<bool>.Success(true);
    }

    private void ReorderChapters()
    {
        for (int i = 0; i < _chapters.Count; i++)
        {
            _chapters[i].UpdateOrderIndex(i + 1);
        }
    }

    #endregion

    #region Metadata Management

    /// <summary>
    /// Обновляет метаданные книги
    /// </summary>
    public void UpdateMetadata(BookMetadata newMetadata)
    {
        Guard.Against.Null(newMetadata, nameof(newMetadata));

        var oldTitle = _metadata.Title;
        _metadata = newMetadata;
        IncrementVersion();

        if (oldTitle != newMetadata.Title)
        {
            AddDomainEvent(new BookTitleChangedEvent(Id, oldTitle, newMetadata.Title));
        }
    }

    /// <summary>
    /// Устанавливает обложку книги
    /// </summary>
    public void SetCoverImage(CoverImage coverImage)
    {
        Guard.Against.Null(coverImage, nameof(coverImage));
        var oldUrl = _coverImage?.Url;
        _coverImage = coverImage;
        IncrementVersion();
        // ИСПРАВЛЕНО: BookCoverUpdatedEvent с правильными параметрами
        AddDomainEvent(new BookCoverUpdatedEvent(Id, oldUrl, coverImage.Url));
    }

    /// <summary>
    /// Устанавливает ISBN
    /// </summary>
    public void SetISBN(BookISBN isbn)
    {
        ISBN = isbn;
        IncrementVersion();
    }

    /// <summary>
    /// Обновляет информацию о публикации
    /// </summary>
    public void UpdatePublicationInfo(PublicationInfo publicationInfo)
    {
        PublicationInfo = publicationInfo ?? PublicationInfo.Create(); // ИСПРАВЛЕНО
        IncrementVersion();
    }

    /// <summary>
    /// Устанавливает год оригинальной публикации
    /// </summary>
    public void SetOriginalPublicationYear(int? year)
    {
        if (year.HasValue)
        {
            Guard.Against.OutOfRange(year.Value, nameof(year), 1, DateTime.UtcNow.Year + 1);
        }
        OriginalPublicationYear = year;
        IncrementVersion();
    }

    #endregion

    #region Genre and Tag Management

    /// <summary>
    /// Добавляет жанр
    /// </summary>
    public void AddGenre(string genre)
    {
        Guard.Against.NullOrWhiteSpace(genre, nameof(genre));
        var normalized = genre.Trim().ToLowerInvariant();
        if (_genres.Add(normalized))
        {
            IncrementVersion();
        }
    }

    /// <summary>
    /// Удаляет жанр
    /// </summary>
    public void RemoveGenre(string genre)
    {
        var normalized = genre.Trim().ToLowerInvariant();
        if (_genres.Remove(normalized))
        {
            IncrementVersion();
        }
    }

    /// <summary>
    /// Очищает все жанры
    /// </summary>
    public void ClearGenres()
    {
        _genres.Clear();
        IncrementVersion();
    }

    /// <summary>
    /// Добавляет тег
    /// </summary>
    public void AddTag(string tag)
    {
        Guard.Against.NullOrWhiteSpace(tag, nameof(tag));
        var normalized = tag.Trim().ToLowerInvariant();
        if (_tags.Add(normalized))
        {
            IncrementVersion();
        }
    }

    /// <summary>
    /// Удаляет тег
    /// </summary>
    public void RemoveTag(string tag)
    {
        var normalized = tag.Trim().ToLowerInvariant();
        if (_tags.Remove(normalized))
        {
            IncrementVersion();
        }
    }

    #endregion

    #region Subject Management

    /// <summary>
    /// Добавляет тему к книге (strongly typed)
    /// </summary>
    public void AddSubject(SubjectId subjectId)
    {
        Guard.Against.Null(subjectId, nameof(subjectId));
        if (_subjectIds.Add(subjectId))
        {
            IncrementVersion();
        }
    }

    /// <summary>
    /// Удаляет тему из книги
    /// </summary>
    public void RemoveSubject(SubjectId subjectId)
    {
        if (_subjectIds.Remove(subjectId))
        {
            IncrementVersion();
        }
    }

    /// <summary>
    /// Добавляет Subject entity (для EF навигации)
    /// </summary>
    public void AddSubject(Subject subject)
    {
        Guard.Against.Null(subject, nameof(subject));
        if (!_bookSubjects.Any(s => s.Id == subject.Id))
        {
            _bookSubjects.Add(subject);
            _subjectIds.Add(subject.Id);
            IncrementVersion();
        }
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Публикует книгу
    /// </summary>
    public Result<bool> Publish()
    {
        if (_status == BookStatus.Published)
        {
            return Result<bool>.Failure(Error.Conflict("Book is already published"));
        }

        if (_chapters.Count == 0)
        {
            return Result<bool>.Failure(Error.Validation("Book must have at least one chapter to be published"));
        }

        _status = BookStatus.Published;
        IncrementVersion();
        AddDomainEvent(new BookPublishedEvent(Id, DateTime.UtcNow));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Снимает книгу с публикации (возвращает в черновик)
    /// </summary>
    public Result<bool> Unpublish()
    {
        if (_status != BookStatus.Published)
        {
            return Result<bool>.Failure(Error.Conflict("Book is not published"));
        }

        _status = BookStatus.Draft;
        IncrementVersion();
        // ИСПРАВЛЕНО: BookUnpublishedEvent с UnpublishedAt
        AddDomainEvent(new BookUnpublishedEvent(Id, DateTime.UtcNow));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Приостанавливает книгу (например, по жалобе)
    /// </summary>
    public Result<bool> Suspend(string reason)
    {
        Guard.Against.NullOrWhiteSpace(reason, nameof(reason));

        if (_status == BookStatus.Suspended)
        {
            return Result<bool>.Failure(Error.Conflict("Book is already suspended"));
        }

        _status = BookStatus.Suspended;
        IncrementVersion();
        // ИСПРАВЛЕНО: используем BookStatusChangedEvent вместо BookSuspendedEvent
        AddDomainEvent(new BookStatusChangedEvent(Id, _status, BookStatus.Suspended));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Архивирует книгу
    /// </summary>
    public Result<bool> Archive()
    {
        if (_status == BookStatus.Archived)
        {
            return Result<bool>.Failure(Error.Conflict("Book is already archived"));
        }

        _status = BookStatus.Archived;
        IncrementVersion();
        AddDomainEvent(new BookArchivedEvent(Id));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Устанавливает статус авторских прав
    /// </summary>
    public void SetCopyrightStatus(CopyrightStatus status)
    {
        CopyrightStatus = status;
        IncrementVersion();
    }

    #endregion

    #region Visualization Management

    /// <summary>
    /// Включает визуализацию для книги
    /// </summary>
    public Result<bool> EnableVisualization(VisualizationSettings? settings = null)
    {
        if (!IsPublished)
        {
            return Result<bool>.Failure(Error.Validation("Book must be published to enable visualization"));
        }

        _visualizationSettings = settings ?? VisualizationSettings.Default();
        _visualizationSettings = _visualizationSettings.Enable();
        IncrementVersion();
        // ИСПРАВЛЕНО: BookVisualizationEnabledEvent с правильными параметрами
        AddDomainEvent(new BookVisualizationEnabledEvent(
            Id,
            _visualizationSettings.PrimaryMode,
            _visualizationSettings.PreferredStyle,
            _visualizationSettings.PreferredProvider));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Выключает визуализацию
    /// </summary>
    public void DisableVisualization()
    {
        _visualizationSettings = _visualizationSettings.Disable();
        IncrementVersion();
        AddDomainEvent(new BookVisualizationDisabledEvent(Id));
    }

    /// <summary>
    /// Устанавливает режим визуализации
    /// </summary>
    public void SetVisualizationMode(VisualizationMode mode)
    {
        _visualizationMode = mode;
        IncrementVersion();
    }

    /// <summary>
    /// Обновляет настройки визуализации
    /// </summary>
    public void UpdateVisualizationSettings(VisualizationSettings settings)
    {
        Guard.Against.Null(settings, nameof(settings));
        _visualizationSettings = settings;
        IncrementVersion();
    }

    #endregion

    #region Statistics and External Source

    /// <summary>
    /// Обновляет статистику книги
    /// </summary>
    public void UpdateStatistics(BookStatistics statistics)
    {
        Guard.Against.Null(statistics, nameof(statistics));
        Statistics = statistics;
        IncrementVersion();
    }

    /// <summary>
    /// Увеличивает счётчик просмотров
    /// </summary>
    public void IncrementViewCount()
    {
        Statistics = Statistics.IncrementViews();
    }

    /// <summary>
    /// Обновляет количество скачиваний (для внешних источников)
    /// </summary>
    public void UpdateDownloadCount(int count)
    {
        Guard.Against.Negative(count, nameof(count));
        DownloadCount = count;
        IncrementVersion();
    }

    /// <summary>
    /// Устанавливает количество слов
    /// </summary>
    public void SetWordCount(int wordCount)
    {
        Guard.Against.Negative(wordCount, nameof(wordCount));
        WordCount = wordCount;
        IncrementVersion();
    }

    /// <summary>
    /// Устанавливает сложность чтения
    /// </summary>
    public void SetReadingDifficulty(double? difficulty)
    {
        ReadingDifficulty = difficulty;
        IncrementVersion();
    }

    /// <summary>
    /// Устанавливает URL полного текста
    /// </summary>
    public void SetFullTextUrl(string? url)
    {
        FullTextUrl = url;
        HasFullText = !string.IsNullOrWhiteSpace(url);
        IncrementVersion();
    }

    /// <summary>
    /// Устанавливает внешние идентификаторы
    /// </summary>
    public void SetExternalIds(ExternalBookId externalIds)
    {
        ExternalIds = externalIds;
        if (externalIds != null)
        {
            Source = externalIds.SourceType.ToBookSource();
        }
        IncrementVersion();
    }

    #endregion
}