using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;
using System.Text.Json;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;
    private readonly ILogger<GetBookByIdQueryHandler> _logger;

    public GetBookByIdQueryHandler(
        IBookRepository bookRepository,
        IMapper mapper,
        IDistributedCache cache,
        ILogger<GetBookByIdQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<BookDto>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"book:{request.BookId}";
        
        // Try to get from cache
        var cachedBook = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedBook))
        {
            _logger.LogDebug("Book found in cache: {BookId}", request.BookId);
            var cachedDto = JsonSerializer.Deserialize<BookDto>(cachedBook);
            return Result<BookDto>.Success(cachedDto!);
        }

        // Get from repository
        var bookId = BookId.From(request.BookId);
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        
        if (book is null)
        {
            return Result<BookDto>.Failure(Error.NotFound($"Book with ID {request.BookId} not found"));
        }

        var bookDto = _mapper.Map<BookDto>(book);

        // Cache the result
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(15),
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1)
        };
        
        await _cache.SetStringAsync(
            cacheKey, 
            JsonSerializer.Serialize(bookDto), 
            cacheOptions, 
            cancellationToken);

        return Result<BookDto>.Success(bookDto);
    }
}
