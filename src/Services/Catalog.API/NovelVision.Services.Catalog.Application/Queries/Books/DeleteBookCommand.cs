using MediatR;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Application.Queries.Books
{
    public record DeleteBookCommand(Guid BookId) : IRequest<Result<bool>>;

}
