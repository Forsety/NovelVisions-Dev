using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using NovelVision.Services.Catalog.Domain.Repositories;
using NovelVision.Services.Catalog.Domain.StronglyTypedIds;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

public class GetBooksByAuthorQueryHandler : IRequestHandler<GetBooksByAuthorQuery, Result<List<BookListDto>>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetBooksByAuthorQueryHandler(
        IBookRepository bookRepository,
        IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<BookListDto>>> Handle(GetBooksByAuthorQuery request, CancellationToken cancellationToken)
    {
        var authorId = AuthorId.From(request.AuthorId);
        var books = await _bookRepository.GetByAuthorAsync(authorId, cancellationToken);
        
        var bookDtos = _mapper.Map<List<BookListDto>>(books);
        return Result<List<BookListDto>>.Success(bookDtos);
    }
}
