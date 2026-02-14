using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Aggregates.BookAggregate;
using NovelVision.Services.Catalog.Domain.Enums;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

public class SearchBooksQueryHandler : IRequestHandler<SearchBooksQuery, Result<PaginatedResultDto<BookListDto>>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchBooksQueryHandler> _logger;

    public SearchBooksQueryHandler(
        IBookRepository bookRepository,
        IMapper mapper,
        ILogger<SearchBooksQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PaginatedResultDto<BookListDto>>> Handle(
        SearchBooksQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Searching books with term: {SearchTerm}, Page: {Page}, AuthorId: {AuthorId}",
                request.SearchTerm, request.PageNumber, request.AuthorId);

            IReadOnlyList<Book> books;
            int totalCount;

            if (request.AuthorId.HasValue)
            {
                // AuthorId filter at DB level
                var authorId = AuthorId.From(request.AuthorId.Value);
                var allAuthorBooks = await _bookRepository.GetByAuthorAsync(authorId, cancellationToken);

                var filtered = allAuthorBooks.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var term = request.SearchTerm.ToLower();
                    filtered = filtered.Where(b =>
                        b.Metadata.Title.ToLower().Contains(term) ||
                        (b.Metadata.Description != null && b.Metadata.Description.ToLower().Contains(term)));
                }

                if (!string.IsNullOrWhiteSpace(request.Genre))
                {
                    var genreLower = request.Genre.ToLower();
                    filtered = filtered.Where(b => b.Genres.Any(g => g.ToLower().Contains(genreLower)));
                }

                if (!string.IsNullOrWhiteSpace(request.Language))
                {
                    filtered = filtered.Where(b => b.Metadata.Language == request.Language);
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (BookStatus.TryFromName(request.Status, ignoreCase: true, out var status))
                    {
                        filtered = filtered.Where(b => b.Status == status);
                    }
                }

                var filteredList = filtered.ToList();
                totalCount = filteredList.Count;

                filteredList = (request.SortBy?.ToLower() switch
                {
                    "title" => request.Descending
                        ? filteredList.OrderByDescending(b => b.Metadata.Title)
                        : filteredList.OrderBy(b => b.Metadata.Title),
                    "created" or "createdat" => request.Descending
                        ? filteredList.OrderByDescending(b => b.CreatedAt)
                        : filteredList.OrderBy(b => b.CreatedAt),
                    _ => filteredList.OrderByDescending(b => b.CreatedAt)
                }).ToList();

                books = filteredList
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
            }
            else
            {
                // General search
                List<string>? genres = null;
                if (!string.IsNullOrWhiteSpace(request.Genre))
                {
                    genres = new List<string> { request.Genre };
                }

                (books, totalCount) = await _bookRepository.SearchAdvancedAsync(
                    searchTerm: request.SearchTerm,
                    genres: genres,
                    languages: !string.IsNullOrWhiteSpace(request.Language)
                        ? new List<string> { request.Language }
                        : null,
                    minPageCount: request.MinPages,
                    maxPageCount: request.MaxPages,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    sortBy: request.SortBy,
                    descending: request.Descending,
                    cancellationToken: cancellationToken);

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (BookStatus.TryFromName(request.Status, ignoreCase: true, out var status))
                    {
                        books = books.Where(b => b.Status == status).ToList();
                    }
                }
            }

            var bookDtos = _mapper.Map<List<BookListDto>>(books);

            var result = new PaginatedResultDto<BookListDto>
            {
                Items = bookDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            _logger.LogInformation("Found {Count} books, total {Total}", books.Count, totalCount);

            return Result<PaginatedResultDto<BookListDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching books");
            return Result<PaginatedResultDto<BookListDto>>.Failure(
                Error.Failure($"Error searching books: {ex.Message}"));
        }
    }
}