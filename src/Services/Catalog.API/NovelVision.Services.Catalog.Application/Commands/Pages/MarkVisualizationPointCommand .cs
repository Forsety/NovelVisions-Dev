// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Pages/MarkVisualizationPointCommand.cs
// ИСПРАВЛЕНИЕ: Используем AutoMapper для маппинга вместо ручного создания DTO
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
/// Команда пометки страницы как точки визуализации
/// </summary>
public record MarkVisualizationPointCommand : IRequest<Result<PageDto>>
{
    public Guid BookId { get; init; }
    public Guid ChapterId { get; init; }
    public Guid PageId { get; init; }
    public bool IsVisualizationPoint { get; init; }
    public string? AuthorHint { get; init; }
}

/// <summary>
/// Обработчик команды пометки точки визуализации
/// </summary>
public sealed class MarkVisualizationPointCommandHandler
    : IRequestHandler<MarkVisualizationPointCommand, Result<PageDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MarkVisualizationPointCommandHandler> _logger;

    public MarkVisualizationPointCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MarkVisualizationPointCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PageDto>> Handle(
        MarkVisualizationPointCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Marking visualization point: BookId={BookId}, PageId={PageId}, IsPoint={IsPoint}",
            request.BookId, request.PageId, request.IsVisualizationPoint);

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

        // Помечаем/снимаем пометку точки визуализации
        if (request.IsVisualizationPoint)
        {
            page.MarkAsVisualizationPoint(request.AuthorHint);
        }
        else
        {
            page.UnmarkAsVisualizationPoint();
        }

        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Visualization point marked: PageId={PageId}", request.PageId);

        // Используем AutoMapper
        var pageDto = _mapper.Map<PageDto>(page);
        return Result<PageDto>.Success(pageDto);
    }
}