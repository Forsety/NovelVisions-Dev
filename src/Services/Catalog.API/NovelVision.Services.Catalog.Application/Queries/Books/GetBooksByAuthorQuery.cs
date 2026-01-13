using System;
using System.Collections.Generic;
using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Queries.Books;

public record GetBooksByAuthorQuery(Guid AuthorId) : IRequest<Result<List<BookListDto>>>;
