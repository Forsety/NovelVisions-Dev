using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public record RemovePageCommand : IRequest<Result<bool>>
{
    public Guid BookId { get; init; }
    public Guid ChapterId { get; init; }
    public Guid PageId { get; init; }
}
