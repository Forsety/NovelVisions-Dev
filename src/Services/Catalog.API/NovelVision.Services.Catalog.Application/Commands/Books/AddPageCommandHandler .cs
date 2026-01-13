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

public class AddPageCommandHandler : IRequestHandler<AddPageCommand, Result<PageDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AddPageCommandHandler> _logger;

    public AddPageCommandHandler(
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddPageCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PageDto>> Handle(AddPageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding page to chapter: {ChapterId}", request.ChapterId);

        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        
        if (book is null)
        {
            return Result<PageDto>.Failure(Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        var chapterId = ChapterId.From(request.ChapterId);
        var chapter = book.Chapters.FirstOrDefault(c => c.Id == chapterId);
        
        if (chapter is null)
        {
            return Result<PageDto>.Failure(Error.NotFound($"Chapter with ID {request.ChapterId} not found"));
        }

        var page = chapter.AddPage(request.Content);

        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var pageDto = _mapper.Map<PageDto>(page);
        
        _logger.LogInformation("Page added successfully: {PageId}", page.Id);
        return Result<PageDto>.Success(pageDto);
    }
}
