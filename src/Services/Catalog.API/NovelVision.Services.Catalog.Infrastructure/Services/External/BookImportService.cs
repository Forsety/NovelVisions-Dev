using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Application.DTOs.Import;
using NovelVision.Services.Catalog.Application.Interfaces;
using NovelVision.Services.Catalog.Domain.Aggregates.AuthorAggregate;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Infrastructure.Services.Import;

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

    #region Single Book Import

    public async Task<ImportBookResultDto> ImportBookAsync(
        int gutenbergId,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ImportOptions();
        var result = await ImportFromGutenbergInternalAsync(gutenbergId, options, cancellationToken);

        if (result.IsFailure)
        {
            throw new Exception($"Import failed: {result.Error.Message}");
        }

        return result.Value;
    }

    public Task<ImportBookResultDto> ImportGutenbergBookAsync(
        int gutenbergId,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return ImportBookAsync(gutenbergId, options, cancellationToken);
    }

    #endregion

    #region Bulk Import

    public async Task<BulkImportResultDto> ImportBooksAsync(
        int[] gutenbergIds,
        ImportOptions? options = null,
        IProgress<BulkImportProgressDto>? progress = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ImportOptions();
        var stopwatch = Stopwatch.StartNew();
        var imported = new List<ImportBookResultDto>();
        var errors = new List<ImportErrorDto>();
        var skipped = 0;

        for (int i = 0; i < gutenbergIds.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = gutenbergIds[i];

            progress?.Report(new BulkImportProgressDto
            {
                Current = i + 1,
                Total = gutenbergIds.Length,
                Successful = imported.Count,
                Failed = errors.Count,
                Skipped = skipped,
                CurrentBookTitle = $"Gutenberg #{id}"
            });

            var result = await ImportFromGutenbergInternalAsync(id, options, cancellationToken);

            if (result.IsSuccess) imported.Add(result.Value);
            else if (result.Error.Type == ErrorType.Conflict) skipped++;
            else
            {
                errors.Add(new ImportErrorDto { GutenbergId = id, ErrorMessage = result.Error.Message });
            }

            await Task.Delay(200, cancellationToken);
        }

        stopwatch.Stop();
        return new BulkImportResultDto
        {
            TotalRequested = gutenbergIds.Length,
            SuccessfulImports = imported.Count,
            SkippedExisting = skipped,
            FailedImports = errors.Count,
            ImportedBooks = imported,
            Errors = errors,
            TotalDuration = stopwatch.Elapsed
        };
    }

    public async Task<BulkImportResultDto> ImportPopularBooksAsync(
        int count,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // ИСПРАВЛЕНИЕ CS1503: Используем объект критериев вместо строки
        var criteria = new GutenbergSearchCriteriaDto { Search = "" };
        var searchResult = await _gutendexService.SearchBooksAsync(criteria, cancellationToken);

        if (searchResult.IsFailure) throw new Exception(searchResult.Error.Message);

        var ids = searchResult.Value.Results.Take(count).Select(b => b.Id).ToArray();
        return await ImportBooksAsync(ids, options, null, cancellationToken);
    }

    public async Task<BulkImportResultDto> ImportBooksByLanguageAsync(
        string language,
        int count,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // ИСПРАВЛЕНИЕ CS1503: Передаем язык через объект критериев
        var criteria = new GutenbergSearchCriteriaDto {
            Languages = new List<string> { language }
        };
        var searchResult = await _gutendexService.SearchBooksAsync(criteria, cancellationToken);

        if (searchResult.IsFailure) throw new Exception(searchResult.Error.Message);

        var ids = searchResult.Value.Results.Take(count).Select(b => b.Id).ToArray();
        return await ImportBooksAsync(ids, options, null, cancellationToken);
    }

    public async Task<BulkImportResultDto> ImportBooksBySubjectAsync(
        string subject,
        int count,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // ИСПРАВЛЕНИЕ CS1503: Передаем тему через Topic
        var criteria = new GutenbergSearchCriteriaDto { Topic = subject };
        var searchResult = await _gutendexService.SearchBooksAsync(criteria, cancellationToken);

        if (searchResult.IsFailure) throw new Exception(searchResult.Error.Message);

        var ids = searchResult.Value.Results.Take(count).Select(b => b.Id).ToArray();
        return await ImportBooksAsync(ids, options, null, cancellationToken);
    }

    #endregion

    #region Sync & Status

    public async Task<ImportBookResultDto> SyncBookAsync(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(BookId.From(bookId), cancellationToken);
        if (book == null) throw new Exception("Book not found");

        var externalId = book.ExternalId;

        // ИСПРАВЛЕНИЕ CS1061: Используем корректные свойства ExternalBookId (обычно это Type и Id или Value)
        // Если свойства называются иначе, используйте те, что определены в домене
        if (externalId == null || externalId.SourceType != ExternalSourceType.Gutenberg)
            throw new Exception("Book is not from Gutenberg");

        if (!int.TryParse(externalId.ExternalId, out int gId))
            throw new Exception("Invalid external ID format");

        return await ImportBookAsync(gId, new ImportOptions { SkipExisting = false }, cancellationToken);
    }

    public async Task<bool> IsAlreadyImportedAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        return await _bookRepository.ExistsByExternalIdAsync(
            ExternalSourceType.Gutenberg,
            gutenbergId.ToString(),
            cancellationToken);
    }

    public async Task<ImportStatusDto?> GetImportStatusAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        var exists = await IsAlreadyImportedAsync(gutenbergId, cancellationToken);
        if (!exists) return new ImportStatusDto { GutenbergId = gutenbergId, IsImported = false };

        // ИСПРАВЛЕНИЕ CS0234: Используем полное имя System.DateTime, чтобы избежать конфликта
        return new ImportStatusDto
        {
            GutenbergId = gutenbergId,
            IsImported = true,
            ImportedAt = System.DateTime.UtcNow
        };
    }

    #endregion

    #region Validation & Preview

    public async Task<bool> CanImportAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        var result = await _gutendexService.GetBookByIdAsync(gutenbergId, cancellationToken);
        return result.IsSuccess;
    }

    public async Task<GutenbergBookDto?> PreviewBookAsync(
        int gutenbergId,
        CancellationToken cancellationToken = default)
    {
        var result = await _gutendexService.GetBookByIdAsync(gutenbergId, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    #endregion

    private async Task<Result<ImportBookResultDto>> ImportFromGutenbergInternalAsync(
        int gutenbergId,
        ImportOptions options,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (options.SkipExisting && await IsAlreadyImportedAsync(gutenbergId, cancellationToken))
            {
                return Result<ImportBookResultDto>.Failure(Error.Conflict($"Book {gutenbergId} exists"));
            }

            var gutendexResult = await _gutendexService.GetBookByIdAsync(gutenbergId, cancellationToken);
            if (gutendexResult.IsFailure) return Result<ImportBookResultDto>.Failure(gutendexResult.Error);

            var gutenbergBook = gutendexResult.Value;
            var (author, isNewAuthor) = await GetOrCreateAuthorAsync(gutenbergBook, options, cancellationToken);

            var metadata = BookMetadata.Create(
                gutenbergBook.Title,
                ExtractDescription(gutenbergBook),
                gutenbergBook.PrimaryLanguage);

            var book = Book.CreateFromExternal(
                metadata,
                author.Id,
                ExternalBookId.CreateGutenberg(gutenbergId.ToString()),
                !string.IsNullOrEmpty(gutenbergBook.CoverImageUrl) && options.DownloadCover
                    ? CoverImage.Create(gutenbergBook.CoverImageUrl, gutenbergBook.CoverImageUrl)
                    : CoverImage.Empty,
                gutenbergBook.DownloadCount,
                CopyrightStatus.PublicDomain).Value;

            int chaptersCount = 0, pagesCount = 0, wordsCount = 0;
            if (options.ImportFullText)
            {
                var textResult = await _gutendexService.DownloadBookTextAsync(gutenbergId, cancellationToken);
                if (textResult.IsSuccess)
                {
                    // ИСПРАВЛЕНИЕ CS1061: Проверьте название метода в GutenbergTextParser.cs
                    // Если метод называется просто Parse, замените ParseBookText на Parse
                    var parseResult = _textParser.Parse(
                        textResult.Value,
                        options.ParseChapters,
                        options.MaxWordsPerPage);

                    foreach (var chData in parseResult.Chapters)
                    {
                        var ch = book.AddChapter(chData.Title, chData.Summary).Value;
                        foreach (var pContent in chData.Pages)
                        {
                            ch.AddPage(pContent);
                            pagesCount++;
                        }
                        chaptersCount++;
                    }
                    wordsCount = parseResult.TotalWordCount;
                }
            }

            await _bookRepository.AddAsync(book, cancellationToken);
            stopwatch.Stop();

            return Result<ImportBookResultDto>.Success(new ImportBookResultDto
            {
                BookId = book.Id.Value,
                GutenbergId = gutenbergId,
                Title = book.Metadata.Title,
                AuthorName = author.DisplayName,
                AuthorId = author.Id.Value,
                IsNewBook = true,
                IsNewAuthor = isNewAuthor,
                ChaptersImported = chaptersCount,
                PagesImported = pagesCount,
                WordCount = wordsCount,
                ImportDuration = stopwatch.Elapsed
            });
        }
        catch (Exception ex)
        {
            return Result<ImportBookResultDto>.Failure(Error.Unexpected(ex.Message));
        }
    }

    private async Task<(Author author, bool isNew)> GetOrCreateAuthorAsync(GutenbergBookDto book, ImportOptions options, CancellationToken ct)
    {
        var authorInfo = book.PrimaryAuthor;
        if (authorInfo == null)
        {
            var unknown = await _authorRepository.GetByEmailAsync("unknown@gutenberg.org", ct);
            if (unknown != null) return (unknown, false);
            var newUnknown = Author.Create("Unknown Author", "unknown@gutenberg.org").Value;
            await _authorRepository.AddAsync(newUnknown, ct);
            return (newUnknown, true);
        }

        var existing = await _authorRepository.FindByGutenbergNameAsync(authorInfo.Name, ct);
        if (existing != null) return (existing, false);

        var newAuthor = Author.CreateFromGutenberg(authorInfo.DisplayName, authorInfo.BirthYear, authorInfo.DeathYear).Value;
        await _authorRepository.AddAsync(newAuthor, ct);
        return (newAuthor, true);
    }

    private static string ExtractDescription(GutenbergBookDto book) =>
        $"Subjects: {string.Join(", ", book.Subjects.Take(3))}. Author: {book.PrimaryAuthor?.DisplayName}";

    private static string CleanSubject(string subject) =>
        subject.Split(new[] { " -- " }, StringSplitOptions.None).Last().Trim();
}