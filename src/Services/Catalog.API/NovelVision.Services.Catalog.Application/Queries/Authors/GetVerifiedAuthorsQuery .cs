using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Authors;

public record GetVerifiedAuthorsQuery : IRequest<Result<List<AuthorListDto>>>;
