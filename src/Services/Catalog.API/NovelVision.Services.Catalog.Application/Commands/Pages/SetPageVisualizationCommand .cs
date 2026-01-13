// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Pages/SetPageVisualizationCommand.cs
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
/// Команда установки визуализации страницы
/// </summary>
public record SetPageVisualizationCommand : IRequest<Result<PageDto>>
{
    public Guid BookId { get; init; }
    public Guid ChapterId { get; init; }
    public Guid PageId { get; init; }
    public string? ImageUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public Guid? VisualizationJobId { get; init; }
}

/// <summary>
/// Обработчик команды установки визуализации страницы
/// </summary>
public sealed class SetPageVisualizationCommandHandler
    : IRequestHandler<SetPageVisualizationCommand, Result<PageDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SetPageVisualizationCommandHandler> _logger;

    public SetPageVisualizationCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SetPageVisualizationCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PageDto>> Handle(
        SetPageVisualizationCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Setting page visualization: BookId={BookId}, ChapterId={ChapterId}, PageId={PageId}",
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

        // Устанавливаем визуализацию
        VisualizationJobId? jobId = request.VisualizationJobId.HasValue
            ? VisualizationJobId.From(request.VisualizationJobId.Value)
            : null;

        page.SetVisualization(request.ImageUrl, request.ThumbnailUrl, jobId);

        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Page visualization set: PageId={PageId}", request.PageId);

        // Используем AutoMapper
        var pageDto = _mapper.Map<PageDto>(page);
        return Result<PageDto>.Success(pageDto);
    }
}