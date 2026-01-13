using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Commands.Authors;

public record DeleteAuthorCommand(Guid AuthorId) : IRequest<Result<bool>>;