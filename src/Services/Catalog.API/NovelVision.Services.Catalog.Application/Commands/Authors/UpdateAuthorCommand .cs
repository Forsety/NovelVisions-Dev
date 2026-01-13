using System;
using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Commands.Authors;

public record UpdateAuthorCommand : IRequest<Result<AuthorDto>>
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Biography { get; init; }
    public Dictionary<string, string> SocialLinks { get; init; } = new();
}


