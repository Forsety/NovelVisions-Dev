using NovelVision.BuildingBlocks.SharedKernel.Primitives;

namespace NovelVision.Services.Visualization.Domain.ValueObjects;

/// <summary>
/// Параметры генерации изображения
/// </summary>
public sealed class GenerationParameters : ValueObject
{
    private GenerationParameters() { }

    private GenerationParameters(
        string size,
        string quality,
        string? aspectRatio,
        int? seed,
        int? steps,
        double? cfgScale,
        string? sampler,
        bool upscale)
    {
        Size = size;
        Quality = quality;
        AspectRatio = aspectRatio;
        Seed = seed;
        Steps = steps;
        CfgScale = cfgScale;
        Sampler = sampler;
        Upscale = upscale;
    }

    /// <summary>
    /// Размер изображения (1024x1024, 1792x1024, etc.)
    /// </summary>
    public string Size { get; private init; } = "1024x1024";

    /// <summary>
    /// Качество (standard, hd, ultra)
    /// </summary>
    public string Quality { get; private init; } = "standard";

    /// <summary>
    /// Соотношение сторон (1:1, 16:9, 2:3, etc.)
    /// </summary>
    public string? AspectRatio { get; private init; }

    /// <summary>
    /// Seed для воспроизводимости (SD, Flux)
    /// </summary>
    public int? Seed { get; private init; }

    /// <summary>
    /// Количество шагов (SD)
    /// </summary>
    public int? Steps { get; private init; }

    /// <summary>
    /// CFG Scale (SD)
    /// </summary>
    public double? CfgScale { get; private init; }

    /// <summary>
    /// Sampler (SD)
    /// </summary>
    public string? Sampler { get; private init; }

    /// <summary>
    /// Применить upscale после генерации
    /// </summary>
    public bool Upscale { get; private init; }

    public static GenerationParameters Default()
    {
        return new GenerationParameters(
            size: "1024x1024",
            quality: "standard",
            aspectRatio: "1:1",
            seed: null,
            steps: null,
            cfgScale: null,
            sampler: null,
            upscale: false);
    }

    public static GenerationParameters Create(
        string? size = null,
        string? quality = null,
        string? aspectRatio = null,
        int? seed = null,
        int? steps = null,
        double? cfgScale = null,
        string? sampler = null,
        bool upscale = false)
    {
        return new GenerationParameters(
            size ?? "1024x1024",
            quality ?? "standard",
            aspectRatio,
            seed,
            steps,
            cfgScale,
            sampler,
            upscale);
    }

    /// <summary>
    /// Параметры для DALL-E 3
    /// </summary>
    public static GenerationParameters ForDallE3(
        string size = "1024x1024",
        string quality = "standard",
        string style = "vivid")
    {
        return new GenerationParameters(
            size,
            quality,
            aspectRatio: null,
            seed: null,
            steps: null,
            cfgScale: null,
            sampler: null,
            upscale: false);
    }

    /// <summary>
    /// Параметры для Stable Diffusion
    /// </summary>
    public static GenerationParameters ForStableDiffusion(
        int steps = 30,
        double cfgScale = 7.5,
        string sampler = "DPM++ 2M Karras",
        int? seed = null)
    {
        return new GenerationParameters(
            size: "512x512",
            quality: "standard",
            aspectRatio: "1:1",
            seed,
            steps,
            cfgScale,
            sampler,
            upscale: false);
    }

    /// <summary>
    /// Параметры для Midjourney
    /// </summary>
    public static GenerationParameters ForMidjourney(
        string aspectRatio = "1:1",
        string quality = "1",
        bool upscale = false)
    {
        return new GenerationParameters(
            size: "1024x1024",
            quality,
            aspectRatio,
            seed: null,
            steps: null,
            cfgScale: null,
            sampler: null,
            upscale);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Size;
        yield return Quality;
        yield return AspectRatio;
        yield return Seed;
        yield return Steps;
        yield return CfgScale;
        yield return Sampler;
        yield return Upscale;
    }
}
