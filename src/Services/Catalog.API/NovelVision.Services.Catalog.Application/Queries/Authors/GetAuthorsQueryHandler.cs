// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Authors/GetAuthorsQueryHandler.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;

namespace NovelVision.Services.Catalog.Application.Queries.Authors;

public class GetAuthorsQueryHandler : IRequestHandler<GetAuthorsQuery, Result<PaginatedResultDto<AuthorListDto>>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public GetAuthorsQueryHandler(
        IAuthorRepository authorRepository,
        IMapper mapper)
    {
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResultDto<AuthorListDto>>> Handle(
        GetAuthorsQuery request,
        CancellationToken cancellationToken)
    {
        // Get queryable from repository
        var query = _authorRepository.GetQueryable();

        // Apply filters
        if (request.Verified.HasValue)
        {
            query = query.Where(a => a.IsVerified == request.Verified.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.DisplayName.ToLower().Contains(searchLower) ||
                (a.Biography != null && a.Biography.ToLower().Contains(searchLower)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var authors = await query
            .OrderByDescending(a => a.IsVerified)
            .ThenBy(a => a.DisplayName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var authorDtos = _mapper.Map<List<AuthorListDto>>(authors);

        // Create paginated result
        var result = new PaginatedResultDto<AuthorListDto>
        {
            Items = authorDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Result<PaginatedResultDto<AuthorListDto>>.Success(result);
    }
}