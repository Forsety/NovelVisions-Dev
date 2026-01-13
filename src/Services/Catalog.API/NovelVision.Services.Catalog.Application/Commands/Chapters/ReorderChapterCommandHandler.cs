// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Chapters/ReorderChapterCommandHandler.cs
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

namespace NovelVision.Services.Catalog.Application.Commands.Chapters;

/// <summary>
/// Handler для переупорядочивания главы в книге
/// </summary>
public sealed class ReorderChapterCommandHandler
    : IRequestHandler<ReorderChapterCommand, Result<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReorderChapterCommandHandler> _logger;

    public ReorderChapterCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        ILogger<ReorderChapterCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ReorderChapterCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Reordering chapter {ChapterId} to position {NewIndex}",
                request.ChapterId,
                request.NewOrderIndex);

            // Найти главу и её книгу
            var chapterId = ChapterId.From(request.ChapterId);

            // Получаем книгу со всеми главами
            var books = await _bookRepository.GetAllAsync(0, 100, cancellationToken);


                        var book = books.FirstOrDefault(b =>
                b.Chapters.Any(c => c.Id == chapterId));

            if (book == null)
            {
                _logger.LogWarning("Chapter {ChapterId} not found in any book", request.ChapterId);
                return Result<bool>.Failure(
                    Error.NotFound($"Chapter with ID {request.ChapterId} not found"));
            }

            var chapter = book.Chapters.First(c => c.Id == chapterId);
            var oldIndex = chapter.OrderIndex;
            var newIndex = request.NewOrderIndex;

            // Валидация нового индекса
            if (newIndex < 1 || newIndex > book.Chapters.Count)
            {
                return Result<bool>.Failure(
                    Error.Validation($"New order index must be between 1 and {book.Chapters.Count}"));
            }

            // Если индекс не изменился - ничего не делаем
            if (oldIndex == newIndex)
            {
                _logger.LogInformation("Chapter already at position {Position}", newIndex);
                return Result<bool>.Success(true);
            }

            // Переупорядочиваем главы
            book.ReorderChapter(chapterId, newIndex);

            // Сохраняем изменения
            await _bookRepository.UpdateAsync(book, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Chapter {ChapterId} moved from position {OldIndex} to {NewIndex}",
                request.ChapterId,
                oldIndex,
                newIndex);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering chapter {ChapterId}", request.ChapterId);
            return Result<bool>.Failure(
                Error.Failure($"Failed to reorder chapter: {ex.Message}"));
        }
    }
}