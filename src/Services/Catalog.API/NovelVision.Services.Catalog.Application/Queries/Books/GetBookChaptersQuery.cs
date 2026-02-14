using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Application.Queries.Books
{
    public record GetBookChaptersQuery(Guid BookId) : IRequest<Result<List<ChapterListDto>>>;

}
