using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Authors;

public record GetAuthorByEmailQuery(string Email) : IRequest<Result<AuthorDto>>;

