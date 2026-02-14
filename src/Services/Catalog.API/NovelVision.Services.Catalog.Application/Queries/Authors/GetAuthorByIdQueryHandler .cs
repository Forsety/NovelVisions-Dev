using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Queries.Authors;

public class GetAuthorByIdQueryHandler : IRequestHandler<GetAuthorByIdQuery, Result<AuthorDto>>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public GetAuthorByIdQueryHandler(
        IAuthorRepository authorRepository,
        IMapper mapper)
    {
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<Result<AuthorDto>> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var authorId = AuthorId.From(request.Id);
        var author = await _authorRepository.GetByIdAsync(authorId, cancellationToken);
        
        if (author is null)
        {
            return Result<AuthorDto>.Failure(Error.NotFound($"Author with ID {request.Id} not found"));
        }

        var authorDto = _mapper.Map<AuthorDto>(author);
        return Result<AuthorDto>.Success(authorDto);
    }
}
