using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;

namespace NovelVision.Services.Catalog.Application.Queries.Authors;

public class GetVerifiedAuthorsQueryHandler : IRequestHandler<GetVerifiedAuthorsQuery, Result<List<AuthorListDto>>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public GetVerifiedAuthorsQueryHandler(
        IAuthorRepository authorRepository,
        IMapper mapper)
    {
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<AuthorListDto>>> Handle(GetVerifiedAuthorsQuery request, CancellationToken cancellationToken)
    {
        var authors = await _authorRepository.GetVerifiedAuthorsAsync(cancellationToken);
        var authorDtos = _mapper.Map<List<AuthorListDto>>(authors);
        return Result<List<AuthorListDto>>.Success(authorDtos);
    }
}
