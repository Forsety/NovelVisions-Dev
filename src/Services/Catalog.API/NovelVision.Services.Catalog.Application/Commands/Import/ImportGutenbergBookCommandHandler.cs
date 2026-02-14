// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Import/ImportGutenbergBookCommandHandler.cs
// С ПОДРОБНЫМ ЛОГИРОВАНИЕМ ДЛЯ ОТЛАДКИ
// ИСПРАВЛЕНО: Добавлено автоматическое включение визуализации для книг Gutenberg
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;
using NovelVision.Services.Catalog.Application.Interfaces;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Entities;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Application.Commands.Import;

public class ImportGutenbergBookCommandHandler
    : IRequestHandler<ImportGutenbergBookCommand, Result<ImportBookResultDto>>
{
    private readonly IGutendexService _gutendexService;
    private readonly ITextParsingService _textParsingService;
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportGutenbergBookCommandHandler> _logger;

    public ImportGutenbergBookCommandHandler(
        IGutendexService gutendexService,
        ITextParsingService textParsingService,
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        ISubjectRepository subjectRepository,
        IUnitOfWork unitOfWork,
        ILogger<ImportGutenbergBookCommandHandler> logger)
    {
        _gutendexService = gutendexService;
        _textParsingService = textParsingService;
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _subjectRepository = subjectRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ImportBookResultDto>> Handle(
        ImportGutenbergBookCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Starting import of Gutenberg book {GutenbergId}. ImportFullText={ImportFullText}, WordsPerPage={WordsPerPage}",
            request.GutenbergId, request.ImportFullText, request.WordsPerPage);

        try
        {
            // 1. Проверяем существование
            if (request.SkipIfExists)
            {
                var exists = await _bookRepository.ExistsByExternalIdAsync(
                    ExternalSourceType.Gutenberg,
                    request.GutenbergId.ToString(),
                    cancellationToken);

                if (exists)
                {
                    _logger.LogInformation("Book {GutenbergId} already exists, skipping", request.GutenbergId);
                    return Result<ImportBookResultDto>.Failure(
                        Error.Conflict($"Book {request.GutenbergId} already imported"));
                }
            }

            // 2. Получаем данные книги из Gutendex
            _logger.LogDebug("Fetching book metadata from Gutendex for {GutenbergId}", request.GutenbergId);

            var bookResult = await _gutendexService.GetBookAsync(request.GutenbergId, cancellationToken);
            if (bookResult.IsFailed)
            {
                _logger.LogWarning("Failed to fetch book {GutenbergId}: {Error}",
                    request.GutenbergId, bookResult.Error?.Message);
                return Result<ImportBookResultDto>.Failure(bookResult.Error ?? Error.NotFound("Book not found"));
            }

            var gutenbergBook = bookResult.Value;
            _logger.LogInformation("Got book metadata: '{Title}' by {Author}",
                gutenbergBook.Title, gutenbergBook.Authors.FirstOrDefault()?.Name ?? "Unknown");

            // 3. Создаём или находим автора
            var (authorId, authorCreated) = await GetOrCreateAuthorAsync(
                gutenbergBook.Authors.FirstOrDefault(),
                request.CreateAuthorIfNotExists,
                cancellationToken);

            if (authorId is null)
            {
                return Result<ImportBookResultDto>.Failure("Could not create or find author");
            }

            _logger.LogDebug("Author: {AuthorId}, Created={Created}", authorId.Value, authorCreated);

            // 4. Создаём метаданные книги
            var metadata = BookMetadata.Create(
                gutenbergBook.Title,
                string.Join("; ", gutenbergBook.Subjects.Take(3)),
                gutenbergBook.PrimaryLanguage);

            // 5. Создаём книгу
            var externalId = ExternalBookId.CreateGutenberg(request.GutenbergId.ToString());

            var bookCreateResult = Book.CreateFromExternalSource(
                metadata,
                authorId,
                externalId,
                !string.IsNullOrEmpty(gutenbergBook.CoverImageUrl)
                    ? CoverImage.Create(gutenbergBook.CoverImageUrl, gutenbergBook.CoverImageUrl)
                    : CoverImage.Empty,
                gutenbergBook.DownloadCount,
                CopyrightStatus.PublicDomain);

            if (bookCreateResult.IsFailed)
            {
                _logger.LogWarning("Failed to create book entity: {Error}", bookCreateResult.Error?.Message);
                return Result<ImportBookResultDto>.Failure(bookCreateResult.Error ?? Error.Failure("Failed to create book"));
            }

            var book = bookCreateResult.Value;
            _logger.LogDebug("Created book entity with ID {BookId}", book.Id.Value);

            // 6. Создаём/находим категории
            var subjectsAssigned = new List<string>();
            if (request.CreateSubjectsIfNotExist)
            {
                foreach (var subject in gutenbergBook.Subjects.Take(10))
                {
                    var subjectEntity = await GetOrCreateSubjectAsync(
                        subject, SubjectType.Topic, cancellationToken);
                    if (subjectEntity is not null)
                    {
                        book.AddSubject(subjectEntity.Id);
                        subjectsAssigned.Add(subject);
                    }
                }

                foreach (var bookshelf in gutenbergBook.Bookshelves.Take(5))
                {
                    var subjectEntity = await GetOrCreateSubjectAsync(
                        bookshelf, SubjectType.Bookshelf, cancellationToken);
                    if (subjectEntity is not null)
                    {
                        book.AddSubject(subjectEntity.Id);
                        subjectsAssigned.Add(bookshelf);
                    }
                }
            }

            _logger.LogDebug("Assigned {Count} subjects", subjectsAssigned.Count);

            // 7. Импортируем текст если требуется
            var chaptersCreated = 0;
            var pagesCreated = 0;
            var wordCount = 0;

            if (request.ImportFullText)
            {
                _logger.LogInformation("Starting text import for book {GutenbergId}...", request.GutenbergId);

                var textResult = await _gutendexService.GetBookTextAsync(
                    request.GutenbergId, cancellationToken);

                if (textResult.IsFailed)
                {
                    _logger.LogWarning(
                        "Failed to download text for book {GutenbergId}: {Error}",
                        request.GutenbergId, textResult.Error?.Message ?? "Unknown error");
                }
                else if (string.IsNullOrEmpty(textResult.Value))
                {
                    _logger.LogWarning("Downloaded text is empty for book {GutenbergId}", request.GutenbergId);
                }
                else
                {
                    _logger.LogInformation(
                        "Downloaded text for book {GutenbergId}: {Length} characters",
                        request.GutenbergId, textResult.Value.Length);

                    var parsedBook = await _textParsingService.ParseBookAsync(
                        textResult.Value,
                        TextFormat.PlainText,
                        request.WordsPerPage,
                        cancellationToken);

                    if (parsedBook.IsFailed)
                    {
                        _logger.LogWarning(
                            "Failed to parse text for book {GutenbergId}: {Error}",
                            request.GutenbergId, parsedBook.Error?.Message ?? "Unknown error");
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Parsed book {GutenbergId}: {ChapterCount} chapters, {PageCount} pages, {WordCount} words",
                            request.GutenbergId,
                            parsedBook.Value.Chapters.Count,
                            parsedBook.Value.TotalPageCount,
                            parsedBook.Value.TotalWordCount);

                        foreach (var parsedChapter in parsedBook.Value.Chapters)
                        {
                            var chapterResult = book.AddChapter(
                                parsedChapter.Title,
                                null);

                            if (chapterResult.IsSucceeded)
                            {
                                chaptersCreated++;
                                var chapter = chapterResult.Value;

                                foreach (var parsedPage in parsedChapter.Pages)
                                {
                                    chapter.AddPage(parsedPage.Content);
                                    pagesCreated++;
                                }
                            }
                            else
                            {
                                _logger.LogWarning(
                                    "Failed to add chapter '{ChapterTitle}': {Error}",
                                    parsedChapter.Title, chapterResult.Error?.Message);
                            }
                        }

                        wordCount = parsedBook.Value.TotalWordCount;
                    }
                }
            }
            else
            {
                _logger.LogDebug("ImportFullText is false, skipping text import");
            }

            _logger.LogInformation(
                "Text import complete: {ChaptersCreated} chapters, {PagesCreated} pages, {WordCount} words",
                chaptersCreated, pagesCreated, wordCount);

            // 8. Публикуем книгу (Gutenberg книги - public domain, сразу публикуем)
            book.Publish();
            _logger.LogDebug("Published book {BookId}", book.Id.Value);

            // 9. НОВОЕ: Включаем визуализацию для книг Gutenberg по умолчанию
            // Все книги из Gutenberg (public domain) должны иметь визуализацию включённой
            try
            {
                var visualizationSettings = VisualizationSettings.UserSelected(
                    style: null,
                    provider: "dalle3",
                    maxImagesPerPage: 3);

                book.UpdateVisualizationSettings(visualizationSettings);

                var enableResult = book.EnableVisualization(visualizationSettings);
                if (enableResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Failed to enable visualization for book {GutenbergId}: {Error}",
                        request.GutenbergId, enableResult.Error?.Message);
                }
                else
                {
                    _logger.LogInformation(
                        "Enabled visualization for Gutenberg book {GutenbergId} with mode {Mode}",
                        request.GutenbergId, visualizationSettings.PrimaryMode.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error enabling visualization for book {GutenbergId}, continuing without visualization",
                    request.GutenbergId);
            }

            // 10. Сохраняем
            await _bookRepository.AddAsync(book, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Successfully imported Gutenberg book {GutenbergId} as {BookId} in {Duration}ms. Visualization enabled: {VisualizationEnabled}",
                request.GutenbergId, book.Id.Value, stopwatch.ElapsedMilliseconds, book.VisualizationSettings?.IsEnabled ?? false);

            return Result<ImportBookResultDto>.Success(new ImportBookResultDto
            {
                Success = true,
                BookId = book.Id.Value,
                GutenbergId = request.GutenbergId,
                Title = gutenbergBook.Title,
                AuthorName = gutenbergBook.Authors.FirstOrDefault()?.Name,
                AuthorId = authorId.Value,
                AuthorCreated = authorCreated,
                ChaptersCreated = chaptersCreated,
                ChaptersImported = chaptersCreated,
                PagesCreated = pagesCreated,
                PagesImported = pagesCreated,
                WordCount = wordCount,
                SubjectsAssigned = subjectsAssigned,
                Subjects = subjectsAssigned,
                Duration = stopwatch.Elapsed,
                ImportDuration = stopwatch.Elapsed
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error importing Gutenberg book {GutenbergId}",
                request.GutenbergId);

            return Result<ImportBookResultDto>.Failure($"Import failed: {ex.Message}");
        }
    }

    private async Task<(AuthorId? AuthorId, bool Created)> GetOrCreateAuthorAsync(
        GutenbergAuthorDto? authorDto,
        bool createIfNotExists,
        CancellationToken cancellationToken)
    {
        if (authorDto is null)
        {
            // Используем Unknown Author - ищем по DisplayName
            var unknownAuthor = await _authorRepository.GetByDisplayNameAsync("Unknown Author", cancellationToken);
            if (unknownAuthor is not null)
            {
                return (unknownAuthor.Id, false);
            }

            if (createIfNotExists)
            {
                // Создаём Unknown Author как Gutenberg автора (без email)
                var newUnknown = Author.CreateFromGutenberg("Unknown Author", null, null, null);
                if (newUnknown.IsSucceeded)
                {
                    await _authorRepository.AddAsync(newUnknown.Value, cancellationToken);
                    _logger.LogInformation("Created Unknown Author with ID {AuthorId}", newUnknown.Value.Id.Value);
                    return (newUnknown.Value.Id, true);
                }
            }

            return (null, false);
        }

        // Сначала пробуем найти по Gutenberg имени (с нормализацией)
        var existingAuthor = await _authorRepository.FindByGutenbergNameAsync(authorDto.Name, cancellationToken);
        if (existingAuthor is not null)
        {
            return (existingAuthor.Id, false);
        }

        // Также пробуем по DisplayName
        existingAuthor = await _authorRepository.GetByDisplayNameAsync(authorDto.Name, cancellationToken);
        if (existingAuthor is not null)
        {
            return (existingAuthor.Id, false);
        }

        if (!createIfNotExists)
        {
            return (null, false);
        }

        // Создаём нового автора из Gutenberg (исторический автор без email)
        var authorResult = Author.CreateFromGutenberg(
            authorDto.Name,
            authorDto.BirthYear,
            authorDto.DeathYear,
            wikipediaUrl: null);

        if (authorResult.IsFailed)
        {
            _logger.LogWarning("Failed to create author '{Name}': {Error}",
                authorDto.Name, authorResult.Error?.Message);
            return (null, false);
        }

        await _authorRepository.AddAsync(authorResult.Value, cancellationToken);
        _logger.LogInformation("Created Gutenberg author '{Name}' with ID {AuthorId}",
            authorDto.Name, authorResult.Value.Id.Value);
        return (authorResult.Value.Id, true);
    }

    private async Task<Subject?> GetOrCreateSubjectAsync(
        string name,
        SubjectType type,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // Очищаем название
        var cleanName = name.Trim();
        if (cleanName.Length > 100)
            cleanName = cleanName.Substring(0, 100);

        // Ищем существующий по имени и типу
        var existing = await _subjectRepository.GetByNameAsync(cleanName, type, cancellationToken);
        if (existing is not null)
            return existing;

        // Создаём новый
        try
        {
            var subject = Subject.Create(cleanName, type);
            await _subjectRepository.AddAsync(subject, cancellationToken);
            return subject;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create subject '{Name}'", cleanName);
            return null;
        }
    }
}