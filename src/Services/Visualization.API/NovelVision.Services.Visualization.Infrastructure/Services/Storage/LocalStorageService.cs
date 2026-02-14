// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Storage/LocalStorageService.cs

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Storage;

/// <summary>
/// Локальная файловая система для хранения изображений (для разработки)
/// </summary>
public sealed class LocalStorageService : IImageStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageService> _logger;
    private readonly HttpClient _httpClient;

    public LocalStorageService(
        IConfiguration configuration,
        ILogger<LocalStorageService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _basePath = configuration["LocalStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration["LocalStorage:BaseUrl"] ?? "/uploads";
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();

        // Создаём директорию если не существует
        Directory.CreateDirectory(_basePath);
    }

    public async Task<Result<ImageMetadata>> UploadImageAsync(
        byte[] imageData,
        string fileName,
        ImageFormat format,
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Создаём структуру папок
            var bookFolder = Path.Combine(_basePath, "books", bookId.ToString(), "images");
            Directory.CreateDirectory(bookFolder);

            var filePath = Path.Combine(bookFolder, fileName);
            await File.WriteAllBytesAsync(filePath, imageData, cancellationToken);

            var relativePath = $"books/{bookId}/images/{fileName}";
            var url = $"{_baseUrl}/{relativePath}";

            // Создаём thumbnail
            var thumbnailUrl = await CreateThumbnailInternalAsync(imageData, bookId, fileName, cancellationToken);

            var (width, height) = GetImageDimensions(imageData);

            var metadata = ImageMetadata.Create(
                url,
                width,
                height,
                imageData.Length,
                format,
                thumbnailUrl,
                relativePath);

            _logger.LogInformation("Saved image locally: {FilePath}", filePath);

            return Result<ImageMetadata>.Success(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image locally: {FileName}", fileName);
            return Result<ImageMetadata>.Failure(Error.Failure($"Failed to save image: {ex.Message}"));
        }
    }

    public async Task<Result<ImageMetadata>> UploadImageFromUrlAsync(
        string sourceUrl,
        string fileName,
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(sourceUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            var format = ImageFormat.FromMimeType(contentType);

            return await UploadImageAsync(imageData, fileName, format, bookId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download and save image from URL: {Url}", sourceUrl);
            return Result<ImageMetadata>.Failure(Error.Failure($"Failed to download image: {ex.Message}"));
        }
    }

    public Task<Result<string>> CreateThumbnailAsync(
        string blobPath,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default)
    {
        // Для локального хранилища - просто возвращаем тот же путь с /thumbnails/
        var thumbnailPath = blobPath.Replace("/images/", "/thumbnails/");
        var url = $"{_baseUrl}/{thumbnailPath}";
        return Task.FromResult(Result<string>.Success(url));
    }

    public Task<Result<bool>> DeleteImageAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_basePath, blobPath.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted local image: {FilePath}", filePath);
            }

            // Удаляем thumbnail
            var thumbnailPath = blobPath.Replace("/images/", "/thumbnails/");
            var thumbFilePath = Path.Combine(_basePath, thumbnailPath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(thumbFilePath))
            {
                File.Delete(thumbFilePath);
            }

            return Task.FromResult(Result<bool>.Success(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete local image: {BlobPath}", blobPath);
            return Task.FromResult(Result<bool>.Failure(Error.Failure($"Failed to delete image: {ex.Message}")));
        }
    }

    public Task<Result<string>> GetImageUrlAsync(
        string blobPath,
        TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl}/{blobPath}";
        return Task.FromResult(Result<string>.Success(url));
    }

    public Task<bool> ExistsAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_basePath, blobPath.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(filePath));
    }

    private async Task<string?> CreateThumbnailInternalAsync(
        byte[] imageData,
        Guid bookId,
        string originalFileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var thumbnailFolder = Path.Combine(_basePath, "books", bookId.ToString(), "thumbnails");
            Directory.CreateDirectory(thumbnailFolder);

            var thumbnailPath = Path.Combine(thumbnailFolder, originalFileName);

            // В реальном проекте здесь должно быть масштабирование
            await File.WriteAllBytesAsync(thumbnailPath, imageData, cancellationToken);

            return $"{_baseUrl}/books/{bookId}/thumbnails/{originalFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create thumbnail locally");
            return null;
        }
    }

    private static (int width, int height) GetImageDimensions(byte[] imageData)
    {
        // PNG signature check
        if (imageData.Length > 24 &&
            imageData[0] == 0x89 && imageData[1] == 0x50 &&
            imageData[2] == 0x4E && imageData[3] == 0x47)
        {
            var width = (imageData[16] << 24) | (imageData[17] << 16) | (imageData[18] << 8) | imageData[19];
            var height = (imageData[20] << 24) | (imageData[21] << 16) | (imageData[22] << 8) | imageData[23];
            return (width, height);
        }

        return (1024, 1024);
    }
}