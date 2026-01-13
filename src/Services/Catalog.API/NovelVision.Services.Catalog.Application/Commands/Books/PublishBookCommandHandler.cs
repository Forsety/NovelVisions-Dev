using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.Services;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public class PublishBookCommandHandler : IRequestHandler<PublishBookCommand, Result<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookDomainService _bookDomainService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PublishBookCommandHandler> _logger;

    public PublishBookCommandHandler(
        IBookRepository bookRepository,
        IBookDomainService bookDomainService,
        IUnitOfWork unitOfWork,
        ILogger<PublishBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _bookDomainService = bookDomainService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(PublishBookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing book with ID: {BookId}", request.BookId);

        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        
        if (book is null)
        {
            return Result<bool>.Failure(Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        // Validate book can be published
        var validationResult = await _bookDomainService.ValidateForPublishingAsync(book, cancellationToken);
        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        // Publish the book
        var publishResult = book.Publish();
        if (publishResult.IsFailed)
        {
            return publishResult;
        }

        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Book published successfully: {BookId}", request.BookId);
        return Result<bool>.Success(true);
    }
}
