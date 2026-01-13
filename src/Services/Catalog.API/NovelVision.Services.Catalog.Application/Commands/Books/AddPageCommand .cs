using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public record AddPageCommand : IRequest<Result<PageDto>>
{
    public Guid BookId { get; init; }
    public Guid ChapterId { get; init; }
    public string Content { get; init; } = string.Empty;
}
