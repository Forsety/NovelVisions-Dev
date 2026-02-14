using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public record SetVisualizationModeCommand : IRequest<Result<bool>>
{
    public Guid BookId { get; init; }
    public string VisualizationMode { get; init; } = string.Empty;
}
