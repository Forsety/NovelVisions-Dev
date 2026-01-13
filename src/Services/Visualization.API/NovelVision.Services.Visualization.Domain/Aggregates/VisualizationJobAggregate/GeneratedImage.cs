using Ardalis.GuardClauses;
using NovelVision.BuildingBlocks.SharedKernel.Primitives;
using NovelVision.Services.Visualization.Domain.Enums;
using NovelVision.Services.Visualization.Domain.StronglyTypedIds;
using NovelVision.Services.Visualization.Domain.ValueObjects;

namespace NovelVision.Services.Visualization.Domain.Aggregates.VisualizationJobAggregate;

/// <summary>
/// Entity: Сгенерированное изображение
/// </summary>
public sealed class GeneratedImage : Entity<GeneratedImageId>
{
    private GeneratedImage() : base(default!) { }

    private GeneratedImage(
        GeneratedImageId id,
        VisualizationJobId jobId,
        ImageMetadata metadata,
        PromptData promptData,
        AIModelProvider provider,
        string? externalJobId) : base(id)
    {
        JobId = jobId;
        Metadata = metadata;
        PromptData = promptData;
        Provider = provider;
        ExternalJobId = externalJobId;
        GeneratedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// ID родительского задания
    /// </summary>
    public VisualizationJobId JobId { get; private init; } = default!;

    /// <summary>
    /// Метаданные изображения
    /// </summary>
    public ImageMetadata Metadata { get; private set; } = default!;

    /// <summary>
    /// Данные промпта использованного для генерации
    /// </summary>
    public PromptData PromptData { get; private init; } = default!;

    /// <summary>
    /// Провайдер AI модели
    /// </summary>
    public AIModelProvider Provider { get; private init; } = AIModelProvider.DallE3;

    /// <summary>
    /// ID задания во внешней системе (DALL-E job id, Midjourney message id, etc.)
    /// </summary>
    public string? ExternalJobId { get; private init; }

    /// <summary>
    /// Время генерации
    /// </summary>
    public DateTime GeneratedAt { get; private init; }

    /// <summary>
    /// Был ли выбран пользователем как основное изображение
    /// </summary>
    public bool IsSelected { get; private set; }

    /// <summary>
    /// Удалено пользователем
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// URL изображения (shortcut)
    /// </summary>
    public string ImageUrl => Metadata.Url;

    public static GeneratedImage Create(
        VisualizationJobId jobId,
        ImageMetadata metadata,
        PromptData promptData,
        AIModelProvider provider,
        string? externalJobId = null)
    {
        Guard.Against.Null(jobId, nameof(jobId));
        Guard.Against.Null(metadata, nameof(metadata));
        Guard.Against.Null(promptData, nameof(promptData));
        Guard.Against.Null(provider, nameof(provider));

        return new GeneratedImage(
            GeneratedImageId.Create(),
            jobId,
            metadata,
            promptData,
            provider,
            externalJobId);
    }

    /// <summary>
    /// Отметить как выбранное
    /// </summary>
    public void Select()
    {
        IsSelected = true;
        UpdateTimestamp();
    }

    /// <summary>
    /// Снять выбор
    /// </summary>
    public void Deselect()
    {
        IsSelected = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Мягкое удаление
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        IsSelected = false;
        UpdateTimestamp();
    }

    /// <summary>
    /// Обновить метаданные (например, после загрузки thumbnail)
    /// </summary>
    public void UpdateMetadata(ImageMetadata newMetadata)
    {
        Guard.Against.Null(newMetadata, nameof(newMetadata));
        Metadata = newMetadata;
        UpdateTimestamp();
    }
}
