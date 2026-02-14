// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Pages/GetPagesForVisualizationQueryHandler.cs
// ИСПРАВЛЕНО под СУЩЕСТВУЮЩУЮ структуру PagesForVisualizationDto.cs:
// - PageVisualizationInfoDto.PageId (не Id)
// - PageVisualizationInfoDto.AuthorHint (не AuthorVisualizationHint)
// - HasVisualization - read-only computed property (не устанавливаем)
// - PagesForVisualizationDto не имеет PreferredStyle/PreferredProvider
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Queries.Pages;

public sealed class GetPagesForVisualizationQueryHandler
    : IRequestHandler<GetPagesForVisualizationQuery, Result<PagesForVisualizationDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetPagesForVisualizationQueryHandler> _logger;

    public GetPagesForVisualizationQueryHandler(
        IBookRepository bookRepository,
        ILogger<GetPagesForVisualizationQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<Result<PagesForVisualizationDto>> Handle(
        GetPagesForVisualizationQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting pages for visualization: BookId={BookId}, OnlyWithout={OnlyWithout}, OnlyPoints={OnlyPoints}",
            request.BookId, request.OnlyWithoutVisualization, request.OnlyVisualizationPoints);

        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);

        if (book is null)
        {
            return Result<PagesForVisualizationDto>.Failure(
                Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        // Счётчик для GlobalPageNumber
        int globalPageNumber = 0;

        // Получаем все страницы с информацией о главах
        // Используем ТОЧНУЮ структуру PageVisualizationInfoDto из репозитория
        var allPages = book.Chapters
            .OrderBy(c => c.OrderIndex)
            .SelectMany(chapter => chapter.Pages
                .OrderBy(p => p.PageNumber)
                .Select(page =>
                {
                    globalPageNumber++;

                    // Определяем VisualizationStatus на основе данных страницы
                    var status = page.HasVisualization
                        ? PageVisualizationStatus.Completed
                        : (page.VisualizationJobId.HasValue
                            ? PageVisualizationStatus.InProgress
                            : PageVisualizationStatus.None);

                    return new PageVisualizationInfoDto
                    {
                        // PageId, не Id!
                        PageId = page.Id.Value,
                        ChapterId = chapter.Id.Value,
                        ChapterNumber = chapter.OrderIndex,
                        ChapterTitle = chapter.Title,
                        PageNumber = page.PageNumber,
                        GlobalPageNumber = globalPageNumber,
                        ContentPreview = page.Content.Length > 200
                            ? page.Content.Substring(0, 200) + "..."
                            : page.Content,
                        WordCount = page.WordCount,
                        // HasVisualization - computed readonly, не устанавливаем!
                        // Устанавливаем VisualizationStatus вместо HasVisualization
                        VisualizationStatus = status,
                        VisualizationImageUrl = page.VisualizationImageUrl,
                        ThumbnailUrl = page.VisualizationThumbnailUrl,
                        VisualizationJobId = page.VisualizationJobId,
                        IsVisualizationPoint = page.IsVisualizationPoint,
                        // AuthorHint, не AuthorVisualizationHint!
                        AuthorHint = page.AuthorVisualizationHint,
                        GeneratedAt = page.VisualizationGeneratedAt
                    };
                }))
            .ToList();

        // Применяем фильтры
        var filteredPages = allPages.AsEnumerable();

        if (request.OnlyWithoutVisualization)
        {
            filteredPages = filteredPages.Where(p => !p.HasVisualization);
        }

        if (request.OnlyVisualizationPoints)
        {
            filteredPages = filteredPages.Where(p => p.IsVisualizationPoint);
        }

        // Null-safe доступ к VisualizationSettings
        var visualizationSettings = book.VisualizationSettings;

        // Используем ТОЧНУЮ структуру PagesForVisualizationDto из репозитория
        // НЕТ PreferredStyle/PreferredProvider в этом DTO!
        var result = new PagesForVisualizationDto
        {
            BookId = book.Id.Value,
            BookTitle = book.Metadata.Title,
            AuthorName = string.Empty, // Будет заполнено отдельно при необходимости
            VisualizationMode = visualizationSettings?.PrimaryMode?.Name ?? "None",
            TotalPages = allPages.Count,
            PagesWithVisualization = allPages.Count(p => p.HasVisualization),
            PagesWithoutVisualization = allPages.Count(p => !p.HasVisualization),
            PagesPendingVisualization = allPages.Count(p => p.VisualizationStatus == PageVisualizationStatus.Pending ||
                                                            p.VisualizationStatus == PageVisualizationStatus.InProgress),
            Pages = filteredPages.ToList()
        };

        return Result<PagesForVisualizationDto>.Success(result);
    }
}