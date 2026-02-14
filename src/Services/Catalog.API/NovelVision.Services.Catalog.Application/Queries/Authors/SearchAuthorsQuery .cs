using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Authors;

public record SearchAuthorsQuery : IRequest<Result<PaginatedResultDto<AuthorListDto>>>
{
    public string? SearchTerm { get; init; }
    public bool? VerifiedOnly { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
