using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Commands.Chapters;

public record CreateChapterCommand : IRequest<Result<ChapterDto>>
{
    public Guid BookId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public int? OrderIndex { get; init; }
}

public record UpdateChapterCommand : IRequest<Result<ChapterDto>>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
}

public record DeleteChapterCommand(Guid ChapterId) : IRequest<Result<bool>>;

public record ReorderChapterCommand : IRequest<Result<bool>>
{
    public Guid ChapterId { get; init; }
    public int NewOrderIndex { get; init; }
}
