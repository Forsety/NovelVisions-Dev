// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Import/BulkImportGutenbergCommandHandler.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs.Import;
using NovelVision.Services.Catalog.Application.Interfaces;

namespace NovelVision.Services.Catalog.Application.Commands.Import;

public class BulkImportGutenbergCommandHandler
    : IRequestHandler<BulkImportGutenbergCommand, Result<BulkImportResultDto>>
{
    private readonly IMediator _mediator;
    private readonly IGutendexService _gutendexService;
    private readonly ILogger<BulkImportGutenbergCommandHandler> _logger;

    public BulkImportGutenbergCommandHandler(
        IMediator mediator,
        IGutendexService gutendexService,
        ILogger<BulkImportGutenbergCommandHandler> logger)
    {
        _mediator = mediator;
        _gutendexService = gutendexService;
        _logger = logger;
    }

    public async Task<Result<BulkImportResultDto>> Handle(
        BulkImportGutenbergCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var startedAt = DateTime.UtcNow;

        _logger.LogInformation("Starting bulk import from Gutenberg");

        var results = new List<ImportBookResultDto>();
        var errors = new List<ImportErrorDto>();
        var authorsCreated = 0;
        var subjectsCreated = 0;

        try
        {
            // Получаем список ID для импорта
            var idsToImport = new List<int>();

            if (request.GutenbergIds.Any())
            {
                idsToImport.AddRange(request.GutenbergIds.Take(request.MaxBooks));
            }
            else if (request.SearchCriteria is not null)
            {
                var searchResult = await _gutendexService.SearchBooksAsync(
                    request.SearchCriteria, cancellationToken);

                if (searchResult.IsSucceeded)
                {
                    idsToImport.AddRange(
                        searchResult.Value.Results
                            .Take(request.MaxBooks)
                            .Select(b => b.Id));
                }
            }
            else
            {
                // По умолчанию - популярные книги
                var popularResult = await _gutendexService.GetPopularBooksAsync(
                    cancellationToken: cancellationToken);

                if (popularResult.IsSucceeded)
                {
                    idsToImport.AddRange(
                        popularResult.Value.Results
                            .Take(request.MaxBooks)
                            .Select(b => b.Id));
                }
            }

            _logger.LogInformation("Found {Count} books to import", idsToImport.Count);

            // Импортируем каждую книгу
            foreach (var gutenbergId in idsToImport)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var importCommand = new ImportGutenbergBookCommand
                    {
                        GutenbergId = gutenbergId,
                        ImportFullText = request.ImportFullText,
                        WordsPerPage = request.WordsPerPage,
                        SkipIfExists = request.SkipExisting,
                        CreateAuthorIfNotExists = true,
                        CreateSubjectsIfNotExist = true,
                        UserId = request.UserId
                    };

                    var result = await _mediator.Send(importCommand, cancellationToken);

                    if (result.IsSucceeded)
                    {
                        results.Add(result.Value);

                        if (result.Value.AuthorCreated)
                            authorsCreated++;
                    }
                    else
                    {
                        errors.Add(new ImportErrorDto
                        {
                            GutenbergId = gutenbergId,
                            ErrorMessage = result.Errors.First().Message,
                            OccurredAt = DateTime.UtcNow
                        });

                        if (!request.ContinueOnError)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing book {GutenbergId}", gutenbergId);

                    errors.Add(new ImportErrorDto
                    {
                        GutenbergId = gutenbergId,
                        ErrorMessage = ex.Message,
                        OccurredAt = DateTime.UtcNow
                    });

                    if (!request.ContinueOnError)
                        break;
                }

                // Задержка между запросами для соблюдения rate limits
                if (request.DelayBetweenRequests > 0)
                {
                    await Task.Delay(request.DelayBetweenRequests, cancellationToken);
                }
            }

            stopwatch.Stop();

            var bulkResult = new BulkImportResultDto
            {
                TotalRequested = idsToImport.Count,
                SuccessCount = results.Count(r => r.Success),
                FailedCount = errors.Count,
                SkippedCount = results.Count(r => r.Success && r.ChaptersCreated == 0),
                AuthorsCreated = authorsCreated,
                SubjectsCreated = subjectsCreated,
                Results = results,
                Errors = errors,
                TotalDuration = stopwatch.Elapsed,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Bulk import completed: {Success} success, {Failed} failed in {Duration}ms",
                bulkResult.SuccessCount, bulkResult.FailedCount, stopwatch.ElapsedMilliseconds);

            return Result<BulkImportResultDto>.Success(bulkResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk import failed");
            return Result<BulkImportResultDto>.Failure($"Bulk import failed: {ex.Message}");
        }
    }
}