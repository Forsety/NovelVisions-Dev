using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Commands.Authors;

public record ChangeAuthorEmailCommand : IRequest<Result<bool>>
{
    public Guid AuthorId { get; init; }
    public string NewEmail { get; init; } = string.Empty;
}
