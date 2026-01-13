using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;

namespace NovelVision.Services.Catalog.Application.Commands.Authors;

public record VerifyAuthorCommand(Guid AuthorId) : IRequest<Result<bool>>;

// ===================================
// src/Services/Catalog.API/NovelVision.Services.Catalog.Application/Commands/Authors/VerifyAuthorCommandHandler.cs
