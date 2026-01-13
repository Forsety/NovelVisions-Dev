using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.Services;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Infrastructure.Services;

public class VisualizationSettingsService : IVisualizationSettingsService
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<VisualizationSettingsService> _logger;

    public VisualizationSettingsService(
        IBookRepository bookRepository,
        ILogger<VisualizationSettingsService> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<Result<VisualizationPlan>> CreateVisualizationPlanAsync(
        BookId bookId,
        VisualizationMode mode,
        CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book == null)
        {
            return Result<VisualizationPlan>.Failure(Error.NotFound($"Book {bookId.Value} not found"));
        }

        if (!book.IsPublished)
        {
            return Result<VisualizationPlan>.Failure(Error.Validation("Book must be published for visualization"));
        }

        var targets = new List<VisualizationTarget>();

        switch (mode.Name)
        {
            case "PerPage":
                var pageIndex = 0;
                foreach (var chapter in book.Chapters.OrderBy(c => c.OrderIndex))
                {
                    foreach (var page in chapter.Pages.OrderBy(p => p.PageNumber))
                    {
                        targets.Add(new VisualizationTarget(
                            Id: page.Id.Value.ToString(),
                            Type: VisualizationTargetType.Page,
                            Content: page.Content.Substring(0, Math.Min(500, page.Content.Length)),
                            OrderIndex: pageIndex++
                        ));
                    }
                }
                break;

            case "PerChapter":
                var chapterIndex = 0;
                foreach (var chapter in book.Chapters.OrderBy(c => c.OrderIndex))
                {
                    var chapterContent = string.Join(" ",
                        chapter.Pages.OrderBy(p => p.PageNumber)
                            .Select(p => p.Content)
                            .Take(3));

                    targets.Add(new VisualizationTarget(
                        Id: chapter.Id.Value.ToString(),
                        Type: VisualizationTargetType.Chapter,
                        Content: chapterContent.Substring(0, Math.Min(1000, chapterContent.Length)),
                        OrderIndex: chapterIndex++
                    ));
                }
                break;

            case "UserSelected":
            case "AuthorDefined":
                _logger.LogInformation("Visualization mode {Mode} requires user/author input", mode.Name);
                break;

            default:
                return Result<VisualizationPlan>.Failure(Error.Validation($"Visualization mode {mode.Name} not supported"));
        }


        var plan = new VisualizationPlan(bookId, mode, targets);
        return Result<VisualizationPlan>.Success(plan);
    }

    public async Task<Result<bool>> CanVisualizeAsync(
        BookId bookId,
        CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book == null)
        {
            return Result<bool>.Failure(Error.NotFound($"Book {bookId.Value} not found"));
        }

        if (!book.IsPublished)
        {
            return Result<bool>.Failure(Error.Validation("Book must be published"));
        }

        if (book.VisualizationMode == VisualizationMode.None)
        {
            return Result<bool>.Failure(Error.Validation("Book visualization is disabled"));
        }

        return Result<bool>.Success(true);
    }
}