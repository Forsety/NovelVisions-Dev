using System;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

public record GetBookByIdQuery(Guid BookId) : IRequest<Result<BookDto>>;
