// src/Services/Visualization.API/NovelVision.Services.Visualization.Infrastructure/Services/Storage/AzureBlobStorageService.cs

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Application.Interfaces;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.ValueObjects;
using NovelVision.Services.Visualization.Infrastructure.Settings;

namespace NovelVision.Services.Visualization.Infrastructure.Services.Storage;

/// <summary>
/// Azure Blob Storage реализация для хранения изображений
/// </summary>
public sealed class AzureBlobStorageService : IImageStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageSettings _settings;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly HttpClient _httpClient;

    public AzureBlobStorageService(
        IOptions<AzureStorageSettings> settings,
        ILogger<AzureBlobStorageService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _logger = logger;
        _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
        _httpClient = httpClientFactory.CreateClient();
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
            // Проверяем размер файла
            var fileSizeMb = imageData.Length / (1024.0 * 1024.0);
            if (fileSizeMb > _settings.MaxFileSizeMb)
            {
                return Result<ImageMetadata>.Failure(
                    Error.Validation($"File size {fileSizeMb:F2}MB exceeds maximum {_settings.MaxFileSizeMb}MB"));
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ImagesContainer);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            // Формируем путь: books/{bookId}/images/{fileName}
            var blobPath = $"books/{bookId}/images/{fileName}";
            var blobClient = containerClient.GetBlobClient(blobPath);

            // Загружаем
            using var stream = new MemoryStream(imageData);
            var headers = new BlobHttpHeaders { ContentType = format.MimeType };

            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);

            // Получаем URL
            var url = GetBlobUrl(blobClient, blobPath);

            // Получаем размеры изображения (простая проверка для PNG/JPEG)
            var (width, height) = GetImageDimensions(imageData);

            // Создаём thumbnail
            var thumbnailUrl = await CreateThumbnailInternalAsync(
                imageData, format, bookId, fileName, cancellationToken);

            var metadata = ImageMetadata.Create(
                url,
                width,
                height,
                imageData.Length,
                format,
                thumbnailUrl,
                blobPath);

            _logger.LogInformation(
                "Uploaded image {FileName} to Azure Blob. Size: {Size} bytes, Path: {Path}",
                fileName, imageData.Length, blobPath);

            return Result<ImageMetadata>.Success(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image {FileName} to Azure Blob", fileName);
            return Result<ImageMetadata>.Failure(Error.Failure($"Failed to upload image: {ex.Message}"));
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
            // Скачиваем изображение
            var response = await _httpClient.GetAsync(sourceUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
            var format = ImageFormat.FromMimeType(contentType);

            return await UploadImageAsync(imageData, fileName, format, bookId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image from URL {Url}", sourceUrl);
            return Result<ImageMetadata>.Failure(Error.Failure($"Failed to download and upload image: {ex.Message}"));
        }
    }

    public async Task<Result<string>> CreateThumbnailAsync(
        string blobPath,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ImagesContainer);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var downloadResult = await blobClient.DownloadContentAsync(cancellationToken);
            var imageData = downloadResult.Value.Content.ToArray();

            // Простое масштабирование (в реальном проекте использовать ImageSharp или SkiaSharp)
            var thumbnailContainer = _blobServiceClient.GetBlobContainerClient(_settings.ThumbnailsContainer);
            await thumbnailContainer.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var thumbnailPath = blobPath.Replace("/images/", "/thumbnails/");
            var thumbnailBlob = thumbnailContainer.GetBlobClient(thumbnailPath);

            using var stream = new MemoryStream(imageData);
            await thumbnailBlob.UploadAsync(stream, true, cancellationToken);

            return Result<string>.Success(GetBlobUrl(thumbnailBlob, thumbnailPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create thumbnail for {BlobPath}", blobPath);
            return Result<string>.Failure(Error.Failure($"Failed to create thumbnail: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> DeleteImageAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ImagesContainer);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var deleted = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            // Также удаляем thumbnail
            var thumbnailContainer = _blobServiceClient.GetBlobContainerClient(_settings.ThumbnailsContainer);
            var thumbnailPath = blobPath.Replace("/images/", "/thumbnails/");
            var thumbnailBlob = thumbnailContainer.GetBlobClient(thumbnailPath);
            await thumbnailBlob.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted image {BlobPath}", blobPath);

            return Result<bool>.Success(deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image {BlobPath}", blobPath);
            return Result<bool>.Failure(Error.Failure($"Failed to delete image: {ex.Message}"));
        }
    }

    public async Task<Result<string>> GetImageUrlAsync(
        string blobPath,
        TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ImagesContainer);
            var blobClient = containerClient.GetBlobClient(blobPath);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return Result<string>.Failure(Error.NotFound($"Image not found: {blobPath}"));
            }

            var url = GetBlobUrl(blobClient, blobPath, expiresIn);
            return Result<string>.Success(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get URL for {BlobPath}", blobPath);
            return Result<string>.Failure(Error.Failure($"Failed to get image URL: {ex.Message}"));
        }
    }

    public async Task<bool> ExistsAsync(
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ImagesContainer);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var response = await blobClient.ExistsAsync(cancellationToken);
            return response.Value;
        }
        catch
        {
            return false;
        }
    }

    private string GetBlobUrl(BlobClient blobClient, string blobPath, TimeSpan? expiresIn = null)
    {
        // Если есть CDN, используем его
        if (!string.IsNullOrEmpty(_settings.CdnBaseUrl))
        {
            return $"{_settings.CdnBaseUrl.TrimEnd('/')}/{blobPath}";
        }

        // Генерируем SAS token
        var sasExpiry = expiresIn ?? TimeSpan.FromHours(_settings.SasTokenExpirationHours);

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(sasExpiry)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        return blobClient.Uri.ToString();
    }

    private async Task<string?> CreateThumbnailInternalAsync(
        byte[] imageData,
        ImageFormat format,
        Guid bookId,
        string originalFileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var thumbnailContainer = _blobServiceClient.GetBlobContainerClient(_settings.ThumbnailsContainer);
            await thumbnailContainer.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var thumbnailPath = $"books/{bookId}/thumbnails/{originalFileName}";
            var thumbnailBlob = thumbnailContainer.GetBlobClient(thumbnailPath);

            // В реальном проекте здесь должно быть масштабирование изображения
            // Пока просто копируем оригинал
            using var stream = new MemoryStream(imageData);
            var headers = new BlobHttpHeaders { ContentType = format.MimeType };
            await thumbnailBlob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, cancellationToken);

            return GetBlobUrl(thumbnailBlob, thumbnailPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create thumbnail for {FileName}", originalFileName);
            return null;
        }
    }

    private static (int width, int height) GetImageDimensions(byte[] imageData)
    {
        // Простое определение размеров для PNG
        if (imageData.Length > 24 &&
            imageData[0] == 0x89 && imageData[1] == 0x50 && // PNG signature
            imageData[2] == 0x4E && imageData[3] == 0x47)
        {
            var width = (imageData[16] << 24) | (imageData[17] << 16) | (imageData[18] << 8) | imageData[19];
            var height = (imageData[20] << 24) | (imageData[21] << 16) | (imageData[22] << 8) | imageData[23];
            return (width, height);
        }

        // Для JPEG и других форматов - возвращаем значения по умолчанию
        return (1024, 1024);
    }
}