// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Pages/ClearPageVisualizationCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Commands.Pages;

/// <summary>
/// Команда очистки визуализации страницы
/// </summary>
public record ClearPageVisualizationCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// ID книги
    /// </summary>
    public Guid BookId { get; init; }

    /// <summary>
    /// ID главы
    /// </summary>
    public Guid ChapterId { get; init; }

    /// <summary>
    /// ID страницы
    /// </summary>
    public Guid PageId { get; init; }
}

/// <summary>
/// Обработчик команды очистки визуализации
/// </summary>
public sealed class ClearPageVisualizationCommandHandler
    : IRequestHandler<ClearPageVisualizationCommand, Result<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClearPageVisualizationCommandHandler> _logger;

    public ClearPageVisualizationCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        ILogger<ClearPageVisualizationCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ClearPageVisualizationCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Clearing visualization for page {PageId}", request.PageId);

        // Получаем книгу с главами и страницами
        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdWithChaptersAsync(bookId, cancellationToken);

        if (book is null)
        {
            _logger.LogWarning("Book {BookId} not found", request.BookId);
            return Result<bool>.Failure(
                Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        // Находим главу
        var chapterId = ChapterId.From(request.ChapterId);
        var chapter = book.Chapters.FirstOrDefault(c => c.Id == chapterId);

        if (chapter is null)
        {
            _logger.LogWarning("Chapter {ChapterId} not found in book {BookId}",
                request.ChapterId, request.BookId);
            return Result<bool>.Failure(
                Error.NotFound($"Chapter with ID {request.ChapterId} not found"));
        }

        // Находим страницу
        var pageId = PageId.From(request.PageId);
        var page = chapter.Pages.FirstOrDefault(p => p.Id == pageId);

        if (page is null)
        {
            _logger.LogWarning("Page {PageId} not found in chapter {ChapterId}",
                request.PageId, request.ChapterId);
            return Result<bool>.Failure(
                Error.NotFound($"Page with ID {request.PageId} not found"));
        }

        // Проверяем, есть ли визуализация
        if (!page.HasVisualization)
        {
            _logger.LogInformation("Page {PageId} has no visualization to clear", request.PageId);
            return Result<bool>.Success(true);
        }

        // Очищаем визуализацию
        page.ClearVisualization();

        _logger.LogInformation("Visualization cleared for page {PageId}", request.PageId);

        // Сохраняем изменения
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}