// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Pages/UpdatePageContentCommand.cs
// ИСПРАВЛЕНИЯ:
// 1. VisualizationImageUrl вместо GeneratedImageUrl (алиас read-only)
// 2. VisualizationThumbnailUrl вместо ThumbnailUrl (алиас read-only)
// 3. EstimatedReadingTimeMinutes вместо EstimatedReadingTime (алиас read-only)
// 4. Конвертация доменного VisualizationStatus в DTO
// 5. Корректная обработка strongly-typed IDs и raw Guids
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Commands.Pages;

/// <summary>
/// Команда обновления контента страницы
/// </summary>
public record UpdatePageContentCommand : IRequest<Result<PageDto>>
{
    public Guid BookId { get; init; }
    public Guid ChapterId { get; init; }
    public Guid PageId { get; init; }
    public string Content { get; init; } = string.Empty;
}

/// <summary>
/// Обработчик команды обновления контента страницы
/// </summary>
public sealed class UpdatePageContentCommandHandler
    : IRequestHandler<UpdatePageContentCommand, Result<PageDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePageContentCommandHandler> _logger;

    public UpdatePageContentCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdatePageContentCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PageDto>> Handle(
        UpdatePageContentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating page content: BookId={BookId}, ChapterId={ChapterId}, PageId={PageId}",
            request.BookId, request.ChapterId, request.PageId);

        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdWithChaptersAsync(bookId, cancellationToken);

        if (book is null)
        {
            return Result<PageDto>.Failure(Error.NotFound($"Book {request.BookId} not found"));
        }

        var chapterId = ChapterId.From(request.ChapterId);
        var chapter = book.Chapters.FirstOrDefault(c => c.Id == chapterId);

        if (chapter is null)
        {
            return Result<PageDto>.Failure(Error.NotFound($"Chapter {request.ChapterId} not found"));
        }

        var pageId = PageId.From(request.PageId);
        var page = chapter.Pages.FirstOrDefault(p => p.Id == pageId);

        if (page is null)
        {
            return Result<PageDto>.Failure(Error.NotFound($"Page {request.PageId} not found"));
        }

        // Обновляем контент
        page.UpdateContent(request.Content);

        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Page content updated: PageId={PageId}", request.PageId);

        // Маппим в DTO через AutoMapper - он знает как работать с strongly-typed IDs
        var pageDto = _mapper.Map<PageDto>(page);
        return Result<PageDto>.Success(pageDto);
    }
}