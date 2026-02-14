// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Queries/Books/GetAuthorByUserIdQueryHandler.cs
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

public class GetAuthorByUserIdQueryHandler : IRequestHandler<GetAuthorByUserIdQuery, Result<AuthorDto>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public GetAuthorByUserIdQueryHandler(
        IAuthorRepository authorRepository,
        IMapper mapper)
    {
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<Result<AuthorDto>> Handle(GetAuthorByUserIdQuery request, CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        
        if (author is null)
        {
            return Result<AuthorDto>.Failure(Error.NotFound($"Author with UserId {request.UserId} not found"));
        }

        var authorDto = _mapper.Map<AuthorDto>(author);
        return Result<AuthorDto>.Success(authorDto);
    }
}