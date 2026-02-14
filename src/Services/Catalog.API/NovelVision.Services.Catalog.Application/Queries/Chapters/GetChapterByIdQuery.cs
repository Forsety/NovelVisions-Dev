using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Chapters;

public record GetChapterByIdQuery(Guid ChapterId) : IRequest<Result<ChapterDto>>;
