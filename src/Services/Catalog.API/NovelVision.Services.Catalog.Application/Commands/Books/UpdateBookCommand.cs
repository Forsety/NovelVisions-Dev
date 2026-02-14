using System;
using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public record UpdateBookCommand : IRequest<Result<BookDto>>
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? Description { get; init; }
    public string? Publisher { get; init; }
    public DateTime? PublicationDate { get; init; }
    public string? Edition { get; init; }
    public List<string> Genres { get; init; } = new();
    public List<string> Tags { get; init; } = new();
}
