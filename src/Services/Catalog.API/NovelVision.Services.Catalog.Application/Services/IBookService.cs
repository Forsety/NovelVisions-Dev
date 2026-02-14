using System;
using System.Threading;
using System.Threading.Tasks;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Catalog.Application.DTOs;

namespace NovelVision.Services.Catalog.Application.Services;

public interface IBookService
{
    Task<Result<BookStatisticsDto>> GetBookStatisticsAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<Result<bool>> ImportBookFromFileAsync(Guid authorId, byte[] fileData, string fileName, CancellationToken cancellationToken = default);
    Task<Result<byte[]>> ExportBookAsync(Guid bookId, string format, CancellationToken cancellationToken = default);
}
