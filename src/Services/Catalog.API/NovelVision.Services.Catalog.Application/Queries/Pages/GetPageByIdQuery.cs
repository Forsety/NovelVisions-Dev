namespace NovelVision.Services.Catalog.Application.Queries.Pages;

using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

public record GetPageByIdQuery(Guid PageId) : IRequest<Result<PageDto>>;

public record GetPageVisualizationQuery(Guid PageId) : IRequest<Result<VisualizationDataDto>>;

public record GetPagesByChapterQuery(Guid ChapterId) : IRequest<Result<List<PageDto>>>;

public record GetPagesCountQuery(Guid ChapterId) : IRequest<Result<int>>; 