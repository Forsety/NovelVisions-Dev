// src/Services/Catalog.API/NovelVision.Services.Catalog.Domain/Events/BookEvents.cs
using System;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Domain.Events;

/// <summary>
/// Событие создания книги
/// </summary>
public sealed record BookCreatedEvent(
    BookId BookId,
    string Title,
    AuthorId AuthorId) : DomainEvent;

/// <summary>
/// Событие добавления главы
/// </summary>
public sealed record ChapterAddedEvent(
    BookId BookId,
    ChapterId ChapterId,
    string Title,
    int OrderIndex) : DomainEvent;

/// <summary>
/// Событие удаления главы
/// </summary>
public sealed record ChapterRemovedEvent(
    BookId BookId,
    ChapterId ChapterId) : DomainEvent;

/// <summary>
/// Событие публикации книги
/// </summary>
public sealed record BookPublishedEvent(
    BookId BookId,
    DateTime PublishedAt) : DomainEvent;

/// <summary>
/// Событие снятия книги с публикации
/// </summary>
public sealed record BookUnpublishedEvent(
    BookId BookId,
    DateTime UnpublishedAt) : DomainEvent;

/// <summary>
/// Событие архивирования книги
/// </summary>
public sealed record BookArchivedEvent(
    BookId BookId) : DomainEvent;

/// <summary>
/// Событие обновления метаданных книги
/// </summary>
public sealed record BookMetadataUpdatedEvent(
    BookId BookId,
    BookMetadata NewMetadata) : DomainEvent;

/// <summary>
/// Событие изменения названия книги
/// </summary>
public sealed record BookTitleChangedEvent(
    BookId BookId,
    string OldTitle,
    string NewTitle) : DomainEvent;

/// <summary>
/// Событие обновления обложки книги
/// </summary>
public sealed record BookCoverUpdatedEvent(
    BookId BookId,
    string? OldCoverUrl,
    string? NewCoverUrl) : DomainEvent;

/// <summary>
/// Событие изменения режима визуализации
/// </summary>
public sealed record BookVisualizationModeChangedEvent(
    BookId BookId,
    VisualizationMode NewMode) : DomainEvent;

/// <summary>
/// Событие включения визуализации
/// </summary>
public sealed record BookVisualizationEnabledEvent(
    BookId BookId,
    VisualizationMode Mode,
    string? PreferredStyle,
    string? PreferredProvider) : DomainEvent;

/// <summary>
/// Событие отключения визуализации
/// </summary>
public sealed record BookVisualizationDisabledEvent(
    BookId BookId) : DomainEvent;

/// <summary>
/// Событие обновления настроек визуализации
/// </summary>
public sealed record BookVisualizationSettingsUpdatedEvent(
    BookId BookId,
    VisualizationSettings NewSettings) : DomainEvent;

/// <summary>
/// Событие импорта книги из внешнего источника
/// </summary>


/// <summary>
/// Событие синхронизации книги с внешним источником
/// </summary>
public sealed record BookSyncedEvent(
    BookId BookId,
    ExternalSourceType SourceType,
    DateTime SyncedAt) : DomainEvent;

/// <summary>
/// Событие добавления жанра
/// </summary>
public sealed record BookGenreAddedEvent(
    BookId BookId,
    string Genre) : DomainEvent;

/// <summary>
/// Событие удаления жанра
/// </summary>
public sealed record BookGenreRemovedEvent(
    BookId BookId,
    string Genre) : DomainEvent;

/// <summary>
/// Событие добавления тега
/// </summary>
public sealed record BookTagAddedEvent(
    BookId BookId,
    string Tag) : DomainEvent;

/// <summary>
/// Событие удаления тега
/// </summary>
public sealed record BookTagRemovedEvent(
    BookId BookId,
    string Tag) : DomainEvent;

/// <summary>
/// Событие изменения статуса книги
/// </summary>
public sealed record BookStatusChangedEvent(
    BookId BookId,
    BookStatus OldStatus,
    BookStatus NewStatus) : DomainEvent;

/// <summary>
/// Событие обновления ISBN
/// </summary>
public sealed record BookIsbnUpdatedEvent(
    BookId BookId,
    BookISBN? OldIsbn,
    BookISBN? NewIsbn) : DomainEvent;

/// <summary>
/// Событие обновления информации о публикации
/// </summary>
public sealed record BookPublicationInfoUpdatedEvent(
    BookId BookId,
    PublicationInfo NewPublicationInfo) : DomainEvent;

/// <summary>
/// Событие добавления категории к книге
/// </summary>
public sealed record BookSubjectAddedEvent(
    BookId BookId,
    SubjectId SubjectId) : DomainEvent;

/// <summary>
/// Событие удаления категории из книги
/// </summary>
public sealed record BookSubjectRemovedEvent(
    BookId BookId,
    SubjectId SubjectId) : DomainEvent;

/// <summary>
/// Событие обновления статистики книги
/// </summary>
public sealed record BookStatisticsUpdatedEvent(
    BookId BookId,
    int OldDownloadCount,
    int NewDownloadCount) : DomainEvent;