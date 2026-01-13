using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    string GenerateFileName(string originalFileName);
    string GetFileUrl(string fileName);
}
