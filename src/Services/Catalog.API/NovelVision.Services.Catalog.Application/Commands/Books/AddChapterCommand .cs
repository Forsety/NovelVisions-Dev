using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Commands.Books;

public record AddChapterCommand : IRequest<Result<ChapterDto>>
{
    public Guid BookId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Summary { get; init; }
}