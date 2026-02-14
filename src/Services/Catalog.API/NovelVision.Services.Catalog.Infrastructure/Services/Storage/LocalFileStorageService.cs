using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NovelVision.Services.Catalog.Application.Common.Interfaces;

namespace NovelVision.Services.Catalog.Infrastructure.Services.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _basePath;

    public LocalFileStorageService(
        IConfiguration configuration,
        ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _basePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uniqueFileName = GenerateFileName(fileName);
        var filePath = Path.Combine(_basePath, uniqueFileName);

        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
        }

        _logger.LogInformation("File saved locally: {FilePath}", filePath);
        return filePath;
    }

    public async Task<Stream> DownloadFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(fileUrl))
        {
            throw new FileNotFoundException($"File not found: {fileUrl}");
        }

        var memory = new MemoryStream();
        using (var stream = new FileStream(fileUrl, FileMode.Open))
        {
            await stream.CopyToAsync(memory, cancellationToken);
        }
        memory.Position = 0;
        return memory;
    }

    public Task<bool> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        if (File.Exists(fileUrl))
        {
            File.Delete(fileUrl);
            _logger.LogInformation("File deleted: {FilePath}", fileUrl);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public string GenerateFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var timestamp = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
        return $"{timestamp}_{guid}{extension}";
    }

    public string GetFileUrl(string fileName)
    {
        return Path.Combine(_basePath, fileName);
    }
}
