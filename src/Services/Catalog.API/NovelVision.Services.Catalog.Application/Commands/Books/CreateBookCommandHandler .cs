// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Books/CreateBookCommandHandler.cs
// »—ѕ–ј¬Ћ≈Ќќ: 
// 1. Book.Create ожидает PublicationInfo? - передаЄм nullable
// 2. Book.Create возвращает Result<Book>, разворачиваем результат
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.BuildingBlocks.SharedKernel.Repositories;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using NovelVision.Services.Catalog.Domain.ValueObjects;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Result<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBookCommandHandler> _logger;

    public CreateBookCommandHandler(
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BookDto>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new book with title: {Title}", request.Title);

        // Validate author exists
        var authorId = AuthorId.From(request.AuthorId);
        var authorExists = await _authorRepository.ExistsAsync(authorId, cancellationToken);
        if (!authorExists)
        {
            return Result<BookDto>.Failure(Error.NotFound($"Author with ID {request.AuthorId} not found"));
        }

        // Check ISBN uniqueness if provided
        if (!string.IsNullOrWhiteSpace(request.ISBN))
        {
            var isbnUnique = await _bookRepository.IsISBNUniqueAsync(request.ISBN, null, cancellationToken);
            if (!isbnUnique)
            {
                return Result<BookDto>.Failure(Error.Conflict($"Book with ISBN {request.ISBN} already exists"));
            }
        }

        // Create value objects
        var language = Language.FromCode(request.LanguageCode);
        var metadata = BookMetadata.Create(
            request.Title,
            request.Subtitle,
            request.Description,
            language,
            0); // Initial page count is 0

        var isbn = !string.IsNullOrWhiteSpace(request.ISBN)
            ? BookISBN.TryCreate(request.ISBN)
            : null;

        // »—ѕ–ј¬Ћ≈Ќќ: Book.Create ожидает PublicationInfo? (nullable)
        // —оздаЄм PublicationInfo только если есть данные, иначе передаЄм null
        PublicationInfo? publicationInfo = null;
        if (!string.IsNullOrWhiteSpace(request.Publisher) ||
            request.PublicationDate.HasValue ||
            !string.IsNullOrWhiteSpace(request.Edition))
        {
            // явное присваивание non-nullable к nullable переменной
            PublicationInfo created = PublicationInfo.Create(
                request.Publisher,
                request.PublicationDate,
                request.Edition);
            publicationInfo = created;
        }

        // Book.Create возвращает Result<Book>
        var bookResult = Book.Create(
            metadata,
            authorId,
            isbn
            );

        if (bookResult.IsFailed)
        {
            _logger.LogError("Failed to create book: {Title}", request.Title);
            return Result<BookDto>.Failure(bookResult.Errors.FirstOrDefault()
                ?? Error.Validation("Failed to create book"));
        }

        var book = bookResult.Value;

        // AddGenre и AddTag возвращают void
        foreach (var genre in request.Genres)
        {
            try
            {
                book.AddGenre(genre);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add genre {Genre}", genre);
            }
        }

        foreach (var tag in request.Tags)
        {
            try
            {
                book.AddTag(tag);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add tag {Tag}", tag);
            }
        }

        // Save to repository
        await _bookRepository.AddAsync(book, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Book created successfully with ID: {BookId}", book.Id.Value);

        // Map to DTO
        var bookDto = _mapper.Map<BookDto>(book);
        return Result<BookDto>.Success(bookDto);
    }
}