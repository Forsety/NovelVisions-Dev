// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/SearchBooksQueryHandler.cs
// ИСПРАВЛЕНИЯ:
// 1. SmartEnum.TryFromName() вместо Enum.TryParse() для CopyrightStatus и BookSource
// 2. Удалены ссылки на IsFreeToUse и SubjectIds (их нет в SearchBooksQuery)
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
            _logger.LogInformation("Searching books with term: {SearchTerm}, Page: {Page}",
                request.SearchTerm, request.PageNumber);

            // Parse CopyrightStatus if provided - используем SmartEnum.TryFromName()
            CopyrightStatus? copyrightStatus = null;
            if (!string.IsNullOrWhiteSpace(request.CopyrightStatus))
            {
                // SmartEnum использует TryFromName вместо Enum.TryParse
                if (CopyrightStatus.TryFromName(request.CopyrightStatus, ignoreCase: true, out var parsedCopyright))
                {
                    copyrightStatus = parsedCopyright;
                }
            }

            // Parse Source if provided - используем SmartEnum.TryFromName()
            BookSource? source = null;
            if (!string.IsNullOrWhiteSpace(request.Source))
            {
                // SmartEnum использует TryFromName вместо Enum.TryParse
                if (BookSource.TryFromName(request.Source, ignoreCase: true, out var parsedSource))
                {
                    source = parsedSource;
                }
            }

            // Parse genres - single genre to list
            List<string>? genres = null;
            if (!string.IsNullOrWhiteSpace(request.Genre))
            {
                genres = new List<string> { request.Genre };
            }

            // Call repository with all filters
            // Примечание: subjectIds = null, т.к. в SearchBooksQuery нет SubjectIds
            var (books, totalCount) = await _bookRepository.SearchAdvancedAsync(
                searchTerm: request.SearchTerm,
                subjectIds: null, // SearchBooksQuery не имеет SubjectIds
                genres: genres,
                languages: !string.IsNullOrWhiteSpace(request.Language)
                    ? new List<string> { request.Language }
                    : null,
                copyrightStatus: copyrightStatus,
                source: source,
                minPageCount: request.MinPages,
                maxPageCount: request.MaxPages,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                sortBy: request.SortBy,
                descending: request.Descending,
                cancellationToken: cancellationToken);

            // Filter by AuthorId if specified (additional filter)
            if (request.AuthorId.HasValue)
            {
                var authorId = AuthorId.From(request.AuthorId.Value);
                books = books.Where(b => b.AuthorId == authorId).ToList();
            }

            // Filter by Status if specified
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                // BookStatus тоже SmartEnum
                if (BookStatus.TryFromName(request.Status, ignoreCase: true, out var status))
                {
                    books = books.Where(b => b.Status == status).ToList();
                }
            }

            // Map to DTOs
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