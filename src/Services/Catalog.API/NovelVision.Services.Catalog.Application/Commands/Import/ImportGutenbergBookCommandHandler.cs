// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Import/ImportGutenbergBookCommandHandler.cs
// ИСПРАВЛЕНИЯ:
// 1. BookMetadata.Create принимает string language, не Language SmartEnum
// 2. Book.CreateFromExternalSource - другая сигнатура (без sourceUrl)
// 3. Book.UpdateDownloadCount вместо SetDownloadCount
// 4. Book.SetCoverImage(CoverImage) вместо SetCoverImageUrl(string)
// 5. ExternalAuthorIdentifiers.Empty (свойство, не метод)
// 6. Author.CreateFromGutenberg вместо CreateFromExternalSource
// 7. Subject.Create - правильные параметры
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
            "Starting import of Gutenberg book {GutenbergId}",
            request.GutenbergId);

        try
        {
            // 1. Проверяем существование книги
            var existingBook = await _bookRepository.GetByGutenbergIdAsync(
                request.GutenbergId, cancellationToken);

            if (existingBook is not null && request.SkipIfExists)
            {
                _logger.LogInformation(
                    "Book {GutenbergId} already exists, skipping",
                    request.GutenbergId);

                return Result<ImportBookResultDto>.Success(new ImportBookResultDto
                {
                    Success = true,
                    BookId = existingBook.Id.Value,
                    GutenbergId = request.GutenbergId,
                    Title = existingBook.Metadata.Title,
                    Duration = stopwatch.Elapsed
                });
            }

            // 2. Получаем данные из Gutendex
            var gutenbergResult = await _gutendexService.GetBookAsync(
                request.GutenbergId, cancellationToken);

            if (gutenbergResult.IsFailed)
            {
                return Result<ImportBookResultDto>.Failure(
                    $"Failed to fetch book from Gutendex: {gutenbergResult.Errors.First().Message}");
            }

            var gutenbergBook = gutenbergResult.Value;

            // 3. Создаём или находим автора
            var (authorId, authorCreated) = await GetOrCreateAuthorAsync(
                gutenbergBook.Authors.FirstOrDefault(),
                request.CreateAuthorIfNotExists,
                cancellationToken);

            if (authorId is null)
            {
                return Result<ImportBookResultDto>.Failure("Author is required but could not be created");
            }

            // 4. Создаём книгу
            // ИСПРАВЛЕНО: BookMetadata.Create принимает string language, не Language SmartEnum
            var languageCode = gutenbergBook.Languages.FirstOrDefault() ?? "en";

            var metadata = BookMetadata.Create(
                title: gutenbergBook.Title,
                subtitle: null,
                description: CreateDescription(gutenbergBook),
                language: languageCode,  // string, не Language!
                pageCount: 1);

            var copyrightStatus = gutenbergBook.Copyright == false
                ? CopyrightStatus.PublicDomain
                : gutenbergBook.Copyright == true
                    ? CopyrightStatus.Copyrighted
                    : CopyrightStatus.Unknown;

            // Создаём ExternalBookId для Gutenberg
            var externalId = ExternalBookId.CreateGutenberg(request.GutenbergId);

            // ИСПРАВЛЕНО: CreateFromExternalSource без sourceUrl параметра
            var bookResult = Book.CreateFromExternalSource(
                metadata,
                authorId,
                externalId,
                coverImage: null,
                downloadCount: gutenbergBook.DownloadCount,
                copyrightStatus: copyrightStatus);

            if (bookResult.IsFailed)
            {
                return Result<ImportBookResultDto>.Failure(
                    $"Failed to create book: {bookResult.Errors.First().Message}");
            }

            var book = bookResult.Value;

            // 5. Устанавливаем дополнительные данные
            // ИСПРАВЛЕНО: UpdateDownloadCount вместо SetDownloadCount
            book.UpdateDownloadCount(gutenbergBook.DownloadCount);

            var coverUrl = await _gutendexService.GetBookCoverUrlAsync(
                request.GutenbergId, cancellationToken);
            if (coverUrl.IsSucceeded && !string.IsNullOrEmpty(coverUrl.Value))
            {
                // ИСПРАВЛЕНО: SetCoverImage(CoverImage) вместо SetCoverImageUrl(string)
                var coverImage = CoverImage.Create(coverUrl.Value);
                book.SetCoverImage(coverImage);
            }

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

            // 7. Импортируем текст если требуется
            var chaptersCreated = 0;
            var pagesCreated = 0;

            if (request.ImportFullText)
            {
                var textResult = await _gutendexService.GetBookTextAsync(
                    request.GutenbergId, cancellationToken);

                if (textResult.IsSucceeded && !string.IsNullOrEmpty(textResult.Value))
                {
                    var parsedBook = await _textParsingService.ParseBookAsync(
                        textResult.Value,
                        TextFormat.PlainText,
                        request.WordsPerPage,
                        cancellationToken);

                    if (parsedBook.IsSucceeded)
                    {
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
                        }
                    }
                }
            }

            // 8. Сохраняем
            await _bookRepository.AddAsync(book, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Successfully imported Gutenberg book {GutenbergId} as {BookId} in {Duration}ms",
                request.GutenbergId, book.Id.Value, stopwatch.ElapsedMilliseconds);

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
                PagesCreated = pagesCreated,
                SubjectsAssigned = subjectsAssigned,
                Duration = stopwatch.Elapsed
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
        GutenbergAuthorDto? gutenbergAuthor,
        bool createIfNotExists,
        CancellationToken cancellationToken)
    {
        if (gutenbergAuthor is null)
        {
            // Создаём Unknown Author
            var unknownAuthor = await _authorRepository.GetByDisplayNameAsync(
                "Unknown Author", cancellationToken);

            if (unknownAuthor is not null)
                return (unknownAuthor.Id, false);

            if (!createIfNotExists)
                return (null, false);

            var unknownResult = Author.Create("Unknown Author", "unknown@gutenberg.local");
            if (unknownResult.IsSucceeded)
            {
                await _authorRepository.AddAsync(unknownResult.Value, cancellationToken);
                return (unknownResult.Value.Id, true);
            }
            return (null, false);
        }

        // Ищем существующего автора
        var existingAuthor = await _authorRepository.GetByDisplayNameAsync(
            gutenbergAuthor.Name, cancellationToken);

        if (existingAuthor is not null)
            return (existingAuthor.Id, false);

        if (!createIfNotExists)
            return (null, false);

        // ИСПРАВЛЕНО: CreateFromGutenberg вместо CreateFromExternalSource
        var authorResult = Author.CreateFromGutenberg(
            gutenbergAuthor.Name,
            gutenbergAuthor.BirthYear,
            gutenbergAuthor.DeathYear,
            wikipediaUrl: null);

        if (authorResult.IsSucceeded)
        {
            await _authorRepository.AddAsync(authorResult.Value, cancellationToken);
            return (authorResult.Value.Id, true);
        }

        return (null, false);
    }

    private async Task<Subject?> GetOrCreateSubjectAsync(
        string name,
        SubjectType type,
        CancellationToken cancellationToken)
    {
        // ИСПРАВЛЕНО: GetByNameAsync принимает только name
        var existing = await _subjectRepository.GetByNameAsync(
            name: name,
            type: type,
            cancellationToken: cancellationToken);
        if (existing is not null)
            return existing;

        // ИСПРАВЛЕНО: Subject.Create с правильными параметрами
        var subject = Subject.Create(name, type, externalMapping: name);
        await _subjectRepository.AddAsync(subject, cancellationToken);
        return subject;
    }

    private static string CreateDescription(GutenbergBookDto book)
    {
        var parts = new List<string>();

        if (book.Subjects.Any())
        {
            parts.Add($"Subjects: {string.Join(", ", book.Subjects.Take(5))}");
        }

        if (book.Bookshelves.Any())
        {
            parts.Add($"Bookshelves: {string.Join(", ", book.Bookshelves.Take(3))}");
        }

        return string.Join(". ", parts);
    }
}