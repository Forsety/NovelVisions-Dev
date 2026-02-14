using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Books
{
    // В файле с Query определениями
    public record GetAuthorByUserIdQuery(Guid UserId) : IRequest<Result<AuthorDto>>;
}
