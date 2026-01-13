using NovelVision.BuildingBlocks.SharedKernel.Results;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Application.Interfaces;

/// <summary>
/// Интерфейс для хранения изображений (Azure Blob / S3)
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Загрузить изображение в хранилище
    /// </summary>
    /// <param name="imageData">Данные изображения</param>
    /// <param name="fileName">Имя файла</param>
    /// <param name="format">Формат изображения</param>
    /// <param name="bookId">ID книги (для организации)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Метаданные загруженного изображения</returns>
    Task<Result<ImageMetadata>> UploadImageAsync(
        byte[] imageData,
        string fileName,
        ImageFormat format,
        Guid bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Загрузить изображение по URL
    /// </summary>
    Task<Result<ImageMetadata>> UploadImageFromUrlAsync(
        string sourceUrl,
        string fileName,
        Guid bookId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать миниатюру
    /// </summary>
    Task<Result<string>> CreateThumbnailAsync(
        string blobPath,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить изображение
    /// </summary>
    Task<Result<bool>> DeleteImageAsync(
        string blobPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить URL для доступа к изображению
    /// </summary>
    Task<Result<string>> GetImageUrlAsync(
        string blobPath,
        TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование изображения
    /// </summary>
    Task<bool> ExistsAsync(
        string blobPath,
        CancellationToken cancellationToken = default);
}
