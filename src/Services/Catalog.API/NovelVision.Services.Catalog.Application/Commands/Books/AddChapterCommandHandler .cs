using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public class AddChapterCommandHandler : IRequestHandler<AddChapterCommand, Result<ChapterDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AddChapterCommandHandler> _logger;

    public AddChapterCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddChapterCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ChapterDto>> Handle(AddChapterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding chapter to book: {BookId}", request.BookId);

        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        
        if (book is null)
        {
            return Result<ChapterDto>.Failure(Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        var chapterResult = book.AddChapter(request.Title, request.Summary);
        if (chapterResult.IsFailed)
        {
            return Result<ChapterDto>.Failure(chapterResult.Errors.First().Message);
        }

        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var chapterDto = _mapper.Map<ChapterDto>(chapterResult.Value);
        
        _logger.LogInformation("Chapter added successfully: {ChapterId}", chapterResult.Value.Id);
        return Result<ChapterDto>.Success(chapterDto);
    }
}
