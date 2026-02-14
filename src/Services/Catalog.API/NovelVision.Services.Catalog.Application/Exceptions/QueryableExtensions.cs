using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static async Task<PaginatedResultDto<T>> ToPaginatedListAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResultDto<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = count,
            TotalPages = (int)System.Math.Ceiling(count / (double)pageSize)
        };
    }
}
