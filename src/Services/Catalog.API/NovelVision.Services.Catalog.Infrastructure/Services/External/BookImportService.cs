// src/Services/Catalog.API/NovelVision.Services.Catalog.Infrastructure/Services/Import/BookImportService.cs
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;
using NovelVision.Services.Catalog.Application.Interfaces;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Infrastructure.Services.Import;

/// <summary>
/// Сервис импорта книг из внешних источников
/// </summary>
public sealed class BookImportService : IBookImportService
{
    private readonly IGutendexService _gutendexService;
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly GutenbergTextParser _textParser;
    private readonly ILogger<BookImportService> _logger;

    public BookImportService(
        IGutendexService gutendexService,
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        GutenbergTextParser textParser,
        ILogger<BookImportService> logger)
    {
        _gutendexService = gutendexService;
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _textParser = textParser;
        _logger = logger;
    }

    public async Task<Result<ImportBookResultDto>> ImportFromGutenbergAsync(
        int gutenbergId,
        ImportOptions options,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var warnings = new List<string>();

        try
        {
            _logger.LogInformation(
                "Starting import of Gutenberg book {Id}", gutenbergId);

            // 1. Проверяем существование
            if (options.SkipExisting)
            {
                var exists = await ExistsByGutenbergIdAsync(gutenbergId, cancellationToken);
                if (exists)
                {
                    _logger.LogInformation(
                        "Book {Id} already exists, skipping", gutenbergId);

                    return Result<ImportBookResultDto>.Failure(
                        Error.Conflict($"Book with Gutenberg ID {gutenbergId} already exists"));
                }
            }

            // 2. Получаем данные из Gutendex
            var bookResult = await _gutendexService.GetBookByIdAsync(
                gutenbergId, cancellationToken);

            if (bookResult.IsFailure)
            {
                return Result<ImportBookResultDto>.Failure(bookResult.Error);
            }

            var gutenbergBook = bookResult.Value;

            // 3. Создаём или находим автора
            var (author, isNewAuthor) = await GetOrCreateAuthorAsync(
                gutenbergBook, options, cancellationToken);

            // 4. Создаём книгу
            var metadata = BookMetadata.Create(
                gutenbergBook.Title,
                ExtractDescription(gutenbergBook),
                gutenbergBook.PrimaryLanguage);

            var externalId = ExternalBookId.CreateGutenberg(gutenbergId.ToString());

            var coverImage = !string.IsNullOrEmpty(gutenbergBook.CoverImageUrl) && options.DownloadCover
                ? CoverImage.Create(gutenbergBook.CoverImageUrl, gutenbergBook.CoverImageUrl)
                : CoverImage.Empty;

            var bookCreateResult = Book.CreateFromExternal(
                metadata,
                author.Id,
                externalId,
                coverImage,
                gutenbergBook.DownloadCount,
                CopyrightStatus.PublicDomain);

            if (bookCreateResult.IsFailure)
            {
                return Result<ImportBookResultDto>.Failure(bookCreateResult.Error);
            }

            var book = bookCreateResult.Value;

            // 5. Добавляем subjects как genres
            foreach (var subject in gutenbergBook.Subjects.Take(5))
            {
                var genre = CleanSubject(subject);
                if (!string.IsNullOrEmpty(genre))
                {
                    book.AddGenre(genre);
                }
            }

            // 6. Импортируем текст если нужно
            int chaptersImported = 0;
            int pagesImported = 0;
            int wordCount = 0;

            if (options.ImportFullText)
            {
                var textResult = await _gutendexService.DownloadBookTextAsync(
                    gutenbergId, cancellationToken);

                if (textResult.IsSuccess)
                {
                    var parseResult = _textParser.ParseBookText(
                        textResult.Value,
                        options.ParseChapters,
                        options.MaxWordsPerPage);

                    foreach (var chapterData in parseResult.Chapters)
                    {
                        var chapterResult = book.AddChapter(chapterData.Title, chapterData.Summary);
                        if (chapterResult.IsSuccess)
                        {
                            var chapter = chapterResult.Value;
                            foreach (var pageContent in chapterData.Pages)
                            {
                                chapter.AddPage(pageContent);
                                pagesImported++;
                            }
                            chaptersImported++;
                        }
                    }

                    wordCount = parseResult.TotalWordCount;
                }
                else
                {
                    warnings.Add($"Could not download full text: {textResult.Error.Message}");
                }
            }

            // 7. Сохраняем
            await _bookRepository.AddAsync(book, cancellationToken);

            stopwatch.Stop();

            var result = new ImportBookResultDto
            {
                BookId = book.Id.Value,
                GutenbergId = gutenbergId,
                Title = book.Metadata.Title,
                AuthorName = author.DisplayName,
                AuthorId = author.Id.Value,
                IsNewBook = true,
                IsNewAuthor = isNewAuthor,
                ChaptersImported = chaptersImported,
                PagesImported = pagesImported,
                WordCount = wordCount,
                HasCover = !string.IsNullOrEmpty(gutenbergBook.CoverImageUrl),
                CoverUrl = gutenbergBook.CoverImageUrl,
                Subjects = gutenbergBook.Subjects,
                Warnings = warnings,
                ImportDuration = stopwatch.Elapsed
            };

            _logger.LogInformation(
                "Successfully imported book {Title} (ID: {BookId}) in {Duration}ms",
                result.Title, result.BookId, stopwatch.ElapsedMilliseconds);

            return Result<ImportBookResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error importing Gutenberg book {Id}", gutenbergId);

            return Result<ImportBookResultDto>.Failure(
                Error.Unexpected($"Import failed: {ex.Message}"));
        }
    }

    public async Task<Result<BulkImportResultDto>> BulkImportFromGutenbergAsync(
        IEnumerable<int> gutenbergIds,
        ImportOptions options,
        IProgress<BulkImportProgressDto>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var ids = gutenbergIds.ToList();
        var imported = new List<ImportBookResultDto>();
        var errors = new List<ImportErrorDto>();
        var skipped = 0;

        for (int i = 0; i < ids.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var gutenbergId = ids[i];

            progress?.Report(new BulkImportProgressDto
            {
                Current = i + 1,
                Total = ids.Count,
                Successful = imported.Count,
                Failed = errors.Count,
                Skipped = skipped,
                CurrentBookTitle = $"Gutenberg #{gutenbergId}"
            });

            var result = await ImportFromGutenbergAsync(
                gutenbergId, options, cancellationToken);

            if (result.IsSuccess)
            {
                imported.Add(result.Value);
            }
            else if (result.Error.Type == ErrorType.Conflict)
            {
                skipped++;
            }
            else
            {
                errors.Add(new ImportErrorDto
                {
                    GutenbergId = gutenbergId,
                    ErrorMessage = result.Error.Message
                });
            }

            // Rate limiting — небольшая пауза между запросами
            await Task.Delay(200, cancellationToken);
        }

        stopwatch.Stop();

        return Result<BulkImportResultDto>.Success(new BulkImportResultDto
        {
            TotalRequested = ids.Count,
            SuccessfulImports = imported.Count,
            SkippedExisting = skipped,
            FailedImports = errors.Count,
            ImportedBooks = imported,
            Errors = errors,
            TotalDuration = stopwatch.Elapsed
        });
    }

    public async Task<Result<BulkImportResultDto>> ImportBySearchAsync(
        string searchQuery,
        int maxBooks,
        ImportOptions options,
        CancellationToken cancellationToken = default)
    {
        var searchResult = await _gutendexService.SearchBooksAsync(
            searchQuery, cancellationToken: cancellationToken);

        if (searchResult.IsFailure)
        {
            return Result<BulkImportResultDto>.Failure(searchResult.Error);
        }

        var ids = searchResult.Value.Results
            .Take(maxBooks)
            .Select(b => b.Id)
            .ToList();

        return await BulkImportFromGutenbergAsync(ids, options, null, cancellationToken);
    }

    public async Task<bool> ExistsByGutenbergIdAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        return await _bookRepository.ExistsByExternalIdAsync(
            ExternalSourceType.Gutenberg,
            gutenbergId.ToString(),
            cancellationToken);
    }

    private async Task<(Author author, bool isNew)> GetOrCreateAuthorAsync(
        GutenbergBookDto book,
        ImportOptions options,
        CancellationToken cancellationToken)
    {
        var authorInfo = book.PrimaryAuthor;

        if (authorInfo == null)
        {
            // Автор неизвестен — используем системного автора
            var unknownAuthor = await _authorRepository.GetByEmailAsync(
                "unknown@gutenberg.org", cancellationToken);

            if (unknownAuthor != null)
            {
                return (unknownAuthor, false);
            }

            // Создаём системного автора
            var createResult = Author.Create("Unknown Author", "unknown@gutenberg.org");
            if (createResult.IsSuccess)
            {
                await _authorRepository.AddAsync(createResult.Value, cancellationToken);
                return (createResult.Value, true);
            }

            throw new InvalidOperationException("Cannot create unknown author");
        }

        // Ищем существующего автора
        var existingAuthor = await _authorRepository.FindByGutenbergNameAsync(
            authorInfo.Name, cancellationToken);

        if (existingAuthor != null)
        {
            return (existingAuthor, false);
        }

        // Создаём нового автора
        if (options.CreateAuthorIfNotExists)
        {
            var result = Author.CreateFromGutenberg(
                authorInfo.DisplayName,
                authorInfo.BirthYear,
                authorInfo.DeathYear);

            if (result.IsSuccess)
            {
                await _authorRepository.AddAsync(result.Value, cancellationToken);
                return (result.Value, true);
            }
        }

        throw new InvalidOperationException(
            $"Author not found and creation disabled: {authorInfo.Name}");
    }

    private static string ExtractDescription(GutenbergBookDto book)
    {
        // Формируем описание из subjects и bookshelves
        var parts = new List<string>();

        if (book.Subjects.Any())
        {
            parts.Add($"Subjects: {string.Join(", ", book.Subjects.Take(3))}");
        }

        if (book.Bookshelves.Any())
        {
            parts.Add($"Bookshelves: {string.Join(", ", book.Bookshelves.Take(3))}");
        }

        if (book.PrimaryAuthor != null && !string.IsNullOrEmpty(book.PrimaryAuthor.LifeYears))
        {
            parts.Add($"Author: {book.PrimaryAuthor.DisplayName} {book.PrimaryAuthor.LifeYears}");
        }

        return string.Join(". ", parts);
    }

    private static string CleanSubject(string subject)
    {
        // "Fiction -- Science Fiction -- Dystopian" -> "Dystopian"
        var parts = subject.Split(new[] { " -- ", " - " }, StringSplitOptions.RemoveEmptyEntries);
        return parts.LastOrDefault()?.Trim() ?? subject;
    }
}